using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RandomEvent
{
  public class Function
  {
    private static readonly Random random = new Random();
    private static readonly List<string> events = new List<string>
        {
            "Death",
            "Marriage",
            "Birth",
            "Hunger",
            "Sickness",
            "Breakages",
            "Salary",
            "Fired from job",
            "Famine",
            "Plague",
            "War",
            "Apocalypse",
            "Inflation"
        };

    private readonly IAmazonDynamoDB dynamoDbClient;

    public Function()
    {
      dynamoDbClient = new AmazonDynamoDBClient();
    }

    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
      var selectedEvent = events[random.Next(events.Count)];
      context.Logger.LogLine($"Selected Event: {selectedEvent}");

      var eventRate = await GetEventRateAsync(selectedEvent);
      context.Logger.LogLine($"Event Rate: {eventRate}");

      return $"{selectedEvent} with rate {eventRate}";
    }

    private async Task<string> GetEventRateAsync(string eventName)
    {
      var request = new GetItemRequest
      {
        TableName = "EventRates",
        Key = new Dictionary<string, AttributeValue>
                {
                    { "EventName", new AttributeValue { S = eventName } }
                }
      };

      var response = await dynamoDbClient.GetItemAsync(request);

      if (response.Item == null || !response.Item.ContainsKey("EventRate"))
      {
        throw new Exception($"Event rate for {eventName} not found");
      }

      return response.Item["EventRate"].S;
    }
  }
}
