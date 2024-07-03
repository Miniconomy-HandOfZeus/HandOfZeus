using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetEventDataForFrontend;

public class Function
{
    private static readonly string tableName = "hand-of-zeus-events"; // Replace with your DynamoDB table name
    private static readonly IAmazonDynamoDB dynamoDbClient = new AmazonDynamoDBClient();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            var scanRequest = new ScanRequest
            {
                TableName = tableName
            };

            var scanResponse = await dynamoDbClient.ScanAsync(scanRequest);

            var items = scanResponse.Items.Select(item =>
                item.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.S)).ToList();

            LambdaLogger.Log("events are: " + items);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                //Body = System.Text.Json.JsonSerializer.Serialize(items),
                Body = JsonConvert.SerializeObject(new { items }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }catch (Exception ex)
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
