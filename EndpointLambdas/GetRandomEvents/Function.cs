using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace GetRandomEvents
{
  public class Function
  {
    private static readonly AmazonDynamoDBClient dynamoDbClient = new AmazonDynamoDBClient();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      var request = new ScanRequest
      {
        TableName = "hand-of-zeus-db",
        FilterExpression = "attribute_exists(event_name)"
      };

      var response = await dynamoDbClient.ScanAsync(request);

      var items = new List<Dictionary<string, string>>();

      foreach (var item in response.Items)
      {
        var eventItem = new Dictionary<string, string>();

        if (item["type"].S == "event")
        {
          eventItem["ID"] = item["Key"].S;
          eventItem["event_name"] = item["event_name"].S;
          eventItem["date"] = item["date"].S;
          eventItem["description"] = item["description"].S;
          eventItem["value"] = item["value"].S;
        }



        items.Add(eventItem);
      }


      return new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonConvert.SerializeObject(items),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };

    }
  }
}
