using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DoesSickPersonaDie;

public class Function
{
    private static readonly Random random = new Random();

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body);
        if (requestBody == null || !requestBody.ContainsKey("personaId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'personaId' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        string personId = requestBody["personaId"];

        // Determine if the person survives or not (50% chance)
        bool survives = random.NextDouble() >= 0.5;

        // Create the response
        var response = new
        {
            personId = personId,
            survives = survives
        };

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(response),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
