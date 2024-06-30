using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FoodPrice.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FoodPrice;

public class Function
{
    private readonly GetPriceFromDB GetPriceFromDB = new GetPriceFromDB();


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {

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
