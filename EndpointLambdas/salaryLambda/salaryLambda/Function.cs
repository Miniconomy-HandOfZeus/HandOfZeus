using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using salaryLambda.services;
using System;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using salaryLambda.Models;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace salaryLambda;

public class Function
{
    
    private readonly WageDeterminationService wageDeterminationService = new WageDeterminationService(new WageService());

    private readonly ServiceProvider _serviceProvider;

    public Function()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddCoreServices();

    }

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(request.Body);
        if (requestBody == null || !requestBody.ContainsKey("personaId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'personaId' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        List<string> personId = requestBody["personaId"];

        List<PersonaWages> personaWages = DetermineWage(personId);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(personaWages),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }


    public List<PersonaWages> DetermineWage(List<string> personas)
    {
        List<PersonaWages> wages = new List<PersonaWages>();

        personas.ForEach(async person =>
        {

            int wage = await wageDeterminationService.DetermineWageAsync();
            wages.Add(new PersonaWages()
            {
                personaId = person,
                wage = wage
            });

        });

        return wages;
    }


}
