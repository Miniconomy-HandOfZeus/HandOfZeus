using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GetValueFromDB.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetValueFromDB;

public class Function
{
    private readonly Repository db = new();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
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

        context.Logger.Log($"Allowed services: {allowedServices}");
        context.Logger.Log($"DB key: {key}");

        // Validate calling service
        if (input.RequestContext.Authorizer.TryGetValue("clientCertCN", out var callingServiceObject))
        {
            string callingService = callingServiceObject?.ToString() ?? string.Empty;
            context.Logger.Log($"{callingService} requested the price");
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

        int price = await db.GetValue(key);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(new { price }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
