using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using EletronicPrice.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EletronicPrice;

public class Function
{
    private readonly List<string> allowedServices = ["electronics_retailer", "zeus"];
    private readonly GetPriceFromDB GetPriceFromDB = new();


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        // Validate calling service
        if (input.RequestContext.Authorizer.TryGetValue("clientCertCN", out var callingServiceObject))
        {
            string callingService = callingServiceObject?.ToString() ?? string.Empty;
            context.Logger.Log($"{callingService} requested the electronics price");
            if (!allowedServices.Contains(callingService))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 403,
                    Body = JsonConvert.SerializeObject(new { message = "Forbidden service" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        } else
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 403,
                Body = JsonConvert.SerializeObject(new { message = "Forbidden" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        int eletronicPrice = await GetEletronicPriceAsync();

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(eletronicPrice),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public async Task<int> GetEletronicPriceAsync()
    {
        int eletronicPrice = await GetPriceFromDB.GetFoodPrice();
        return eletronicPrice;
    }
}
