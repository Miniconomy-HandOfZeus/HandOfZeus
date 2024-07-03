using Amazon.Lambda.Core;
using SalaryLambdas.services;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using SalaryLambdas.Models;
using System.Numerics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SalaryLambdas;

public class Function
{
    private readonly WageDeterminationService wageDeterminationService = new WageDeterminationService(new WageService());

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        //if (input.QueryStringParameters == null)
        //{
        //    return new APIGatewayProxyResponse
        //    {
        //        StatusCode = 500,
        //        Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
        //        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //    };
        //}

        // Parse the input body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, List<BigInteger>>>(input.Body);
        if (requestBody == null || !requestBody.ContainsKey("people"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'people' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        List<BigInteger> personId = requestBody["people"];

        List<PersonaWages> personaWages = DetermineWage(personId);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(personaWages),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }


    public List<PersonaWages> DetermineWage(List<BigInteger> personas)
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
