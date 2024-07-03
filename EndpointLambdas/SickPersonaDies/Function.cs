using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using SickPersonaDies.Models;
using SickPersonaDies.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SickPersonaDies;

public class Function
{
    private static readonly Random random = new Random();
    private static readonly RequestHandler RequestHandler = new RequestHandler();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, long>>(input.Body);
        if (requestBody == null || !requestBody.ContainsKey("personaId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'personaId' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        long personId = requestBody["personaId"];

        // Determine if the person survives or not (50% chance)
        bool survives = random.NextDouble() >= 0.5;

        // Create the response
        SickPersonDiesClass response = new()
        {
            personID = personId,
            survives = survives
        };

        if (!response.survives)
        {
            try
            {
                await RequestHandler.SendPostRequestAsync(response);
            } 
            catch (Exception ex)
            {
                context.Logger.Log($"Error: {ex.Message}");
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(response),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
