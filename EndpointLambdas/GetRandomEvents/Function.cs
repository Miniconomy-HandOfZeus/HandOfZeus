using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace GetRandomEvents
{
  public class Function
  {
    //private static readonly AmazonDynamoDBClient dynamoDbClient = new AmazonDynamoDBClient();

        //public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        //{
        //        try
        //        {
        //            var request = new ScanRequest
        //            {
        //                TableName = "hand-of-zeus-db",
        //                FilterExpression = "attribute_exists(event_name)"
        //            };

        //            var response = await dynamoDbClient.ScanAsync(request);

        //            var items = new List<Dictionary<string, string>>();

        //            foreach (var item in response.Items)
        //            {
        //                var eventItem = new Dictionary<string, string>();

        //                if (item["type"].S == "event")
        //                {
        //                    eventItem["ID"] = item["Key"].S;
        //                    eventItem["event_name"] = item["event_name"].S;
        //                    eventItem["date"] = item["date"].S;
        //                    eventItem["description"] = item["description"].S;
        //                    eventItem["value"] = item["value"].S;
        //                }



        //                items.Add(eventItem);

        //            }

        //            return new APIGatewayProxyResponse
        //            {
        //                StatusCode = 200,
        //                Body = JsonConvert.SerializeObject(items),
        //                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            LambdaLogger.Log("there was an error when trying to get events from db: " + ex.Message);
        //            return new APIGatewayProxyResponse
        //            {
        //                StatusCode = 500,
        //                Body = JsonConvert.SerializeObject(ex.Message),
        //                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //            };
        //        }




        //}
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
                LambdaLogger.Log("events are: " + items.ToString());
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    //Body = System.Text.Json.JsonSerializer.Serialize(items),
                    Body = JsonConvert.SerializeObject(new { items }),
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
}
