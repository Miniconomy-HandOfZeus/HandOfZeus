using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Function
{
  private static readonly AmazonDynamoDBClient dynamoDbClient = new AmazonDynamoDBClient();

  public async Task<string> FunctionHandler(ILambdaContext context)
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

      if (item.ContainsKey("event_name") && item.ContainsKey("date"))
      {
        eventItem["event_name"] = item["event_name"].S;
        eventItem["date"] = item["date"].S;
      }

      items.Add(eventItem);
    }

    return JsonConvert.SerializeObject(items);
  }
}
