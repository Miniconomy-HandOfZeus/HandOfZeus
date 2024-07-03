using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HasSimStarted.Service;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HasSimStarted;

public class Function
{
    
   private readonly DBHelper dbHelper = new DBHelper();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {

            bool hasStarted = await dbHelper.GetValue("hasStarted");


            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(new { hasStarted }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = System.Text.Json.JsonSerializer.Serialize(ex.Message),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

    }
}
