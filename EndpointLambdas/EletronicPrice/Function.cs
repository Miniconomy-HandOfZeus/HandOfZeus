using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using EletronicPrice.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EletronicPrice;

public class Function
{

    private readonly GetPriceFromDB GetPriceFromDB = new GetPriceFromDB();


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Parse the request body to get the person ID

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
