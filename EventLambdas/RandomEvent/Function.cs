using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

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

    private static readonly List<long> people = new List<long>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
            31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50
        };

    private readonly IAmazonDynamoDB dynamoDbClient;
    private static readonly HttpClient httpClient = new HttpClient();

    public Function()
    {
      dynamoDbClient = new AmazonDynamoDBClient();
    }

    public async Task FunctionHandler(string input, ILambdaContext context)
    {
      var selectedEvent = events[random.Next(events.Count)];
      context.Logger.LogLine($"Selected Event: {selectedEvent}");

      var eventRate = await GetEventRateAsync(selectedEvent);
      context.Logger.LogLine($"Event Rate: {eventRate}");

      // For illustration, randomly selecting affected people based on event rate
      var affectedPeopleCount = GetAffectedPeopleCount(eventRate);
      var affectedPeople = GetRandomPeople(affectedPeopleCount);
      context.Logger.LogLine($"Affected People: {string.Join(", ", affectedPeople)}");

      // Call the other service's endpoint
      await CallServiceEndpointAsync(selectedEvent, eventRate, affectedPeople);
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

    private int GetAffectedPeopleCount(string eventRate)
    {
      // This method converts the event rate into an integer count.
      // Adjust the logic here based on the actual structure of your event rates.
      if (int.TryParse(eventRate, out int count))
      {
        return count;
      }
      else if (eventRate.EndsWith("%"))
      {
        double percentage = double.Parse(eventRate.TrimEnd('%')) / 100;
        return (int)(percentage * people.Count);
      }
      else
      {
        return 0; // Default fallback
      }
    }

    private List<long> GetRandomPeople(int count)
    {
      var selectedPeople = new List<long>();
      var availablePeople = new List<long>(people);

      for (int i = 0; i < count && availablePeople.Count > 0; i++)
      {
        int index = random.Next(availablePeople.Count);
        selectedPeople.Add(availablePeople[index]);
        availablePeople.RemoveAt(index);
      }

      return selectedPeople;
    }

    private async Task CallServiceEndpointAsync(string eventName, string eventRate, List<long> affectedPeople)
    {
      var requestBody = new
      {
        EventName = eventName,
        EventRate = eventRate,
        AffectedPeople = affectedPeople
      };

      // Replace with the actual service endpoint URL
      string serviceEndpointUrl = "https://your-service-endpoint-url.com/event";

      var response = await httpClient.PostAsJsonAsync(serviceEndpointUrl, requestBody);

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to call service endpoint. Status code: {response.StatusCode}");
      }
    }
  }
}
