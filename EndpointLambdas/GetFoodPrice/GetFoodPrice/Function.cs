using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GetFoodPrice.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetFoodPrice;

public class Function
{
    private readonly GetPriceFromDB GetPriceFromDB;


    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Parse the request body to get the person ID

        int foodPrice = await GetFoodPriceAsync();

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(foodPrice),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public async Task<int> GetFoodPriceAsync()
    {
        int foodPrice = await GetPriceFromDB.GetFoodPrice();
        return foodPrice;
    }

}
