using Amazon.Lambda.Core;
using SalaryLambdas.services;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using SalaryLambdas.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SalaryLambdas;

public class Function
{
    private readonly WageDeterminationService wageDeterminationService = new WageDeterminationService(new WageService());

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        if (input.QueryStringParameters == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        // Parse the custom query parameters set by API gateway
        if (!input.QueryStringParameters.TryGetValue("allowed_services", out string? allowedServicesString) || !input.QueryStringParameters.TryGetValue("key", out string? key))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        List<string> allowedServices = [.. allowedServicesString.Split(",")];

        context.Logger.Log($"Allowed services: {string.Join(", ", allowedServices)}");
        context.Logger.Log($"DB key: {key}");

        // Validate calling service
        if (input.RequestContext.Authorizer.TryGetValue("clientCertCN", out var callingServiceObject))
        {
            string callingService = callingServiceObject?.ToString() ?? string.Empty;
            context.Logger.Log($"{callingService} called this endpoint!");
            if (!allowedServices.Contains(callingService))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 403,
                    Body = JsonConvert.SerializeObject(new { message = "Forbidden service" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }
        else
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 403,
                Body = JsonConvert.SerializeObject(new { message = "Forbidden" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        // Parse the input body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(input.Body);
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
