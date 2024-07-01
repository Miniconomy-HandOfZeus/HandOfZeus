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
        "FamineStart",
        "FamineEnd",
        "PlagueStart",
        "PlagueEnd",
        "WarStart",
        "WarEnd",
        "Apocalypse",
        "Inflation"
    };


    private static readonly Dictionary<string, int> eventWeights = new Dictionary<string, int>
    {
        { "Death", 10 },
        { "Marriage", 20 },
        { "Birth", 30 },
        { "Hunger", 15 },
        { "Sickness", 15 },
        { "Breakages", 5 },
        { "Salary", 25 },
        { "Fired from job", 5 },
        { "FamineStart", 1 },
        { "FamineEnd", 0 },
        { "PlagueStart", 1 },
        { "PlagueEnd", 0 },
        { "WarStart", 1 },
        { "WarEnd", 0 },
        { "Apocalypse", 1 },
        { "Inflation", 10 }
    };

    private static readonly Dictionary<string, string[]> eventEndpoints = new Dictionary<string, string[]>
    {
        { "Death", new[] { "https://persona.projects.bbdgrad.com" } },
        { "Marriage", new[] { "https://persona.projects.bbdgrad.com" } },
        { "Birth", new[] { "https://persona.projects.bbdgrad.com" } },
        { "Hunger", new[] { "https://sustenance.projects.bbdgrad.com" } },
        { "Sickness", new[] { "https://api.health.projects.bbdgrad.com", "https://persona.projects.bbdgrad.com" } },
        { "Breakages", new[] { "https://api.insurance.projects.bbdgrad.com" } },
        { "Salary", new[] { "https://labour.projects.bbdgrad.com" } },
        { "Fired from job", new[] { "https://labour.projects.bbdgrad.com" } },
        { "FamineStart", new[] { "https://sustenance.projects.bbdgrad.com" } },
        { "FamineEnd", new[] { "https://sustenance.projects.bbdgrad.com" } },
        { "PlagueStart", new[] { "https://api.health.projects.bbdgrad.com", "https://persona.projects.bbdgrad.com" } },
        { "PlagueEnd", new[] { "https://api.health.projects.bbdgrad.com", "https://persona.projects.bbdgrad.com" } },
        { "WarStart", new string[] { } }, // No notification for start
        { "WarEnd", new string[] { } }, // No notification for end
        { "Apocalypse", new[] { "https://persona.projects.bbdgrad.com" } },
        { "Inflation", new[] { "https://api.commercialbank.projects.bbdgrad.com" } }
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
      var selectedEvent = SelectWeightedRandomEvent();
      context.Logger.LogLine($"Selected Event: {selectedEvent}");

      var eventRate = await GetEventRateAsync(selectedEvent);
      context.Logger.LogLine($"Event Rate: {eventRate}");


      var affectedPeopleCount = GetAffectedPeopleCount(eventRate);
      var affectedPeople = GetRandomPeople(affectedPeopleCount);

      context.Logger.LogLine($"Affected People: {string.Join(", ", affectedPeople)}");

      await CallServiceEndpointsAsync(selectedEvent, eventRate, affectedPeople);

      // Handle start and end events logic
      if (selectedEvent.EndsWith("Start"))
      {
        var endEvent = selectedEvent.Replace("Start", "End");
        eventWeights[endEvent] = 100; // Increase end event weight
        await UpdateEventRateInDynamoDB(endEvent, "100");
      }
      else if (selectedEvent.EndsWith("End"))
      {
        eventWeights[selectedEvent] = 0; // Set end event weight to 0 after ending
        await UpdateEventRateInDynamoDB(selectedEvent, "0");
      }

      if (selectedEvent == "FamineStart" || selectedEvent == "FamineEnd")
      {
        await UpdateEventRateInDynamoDB("Hunger", selectedEvent == "FamineStart" ? "50" : "15");
        await UpdateEventRateInDynamoDB("Inflation", selectedEvent == "FamineStart" ? "20" : "10");
      }
      else if (selectedEvent == "PlagueStart" || selectedEvent == "PlagueEnd")
      {
        await UpdateEventRateInDynamoDB("Sickness", selectedEvent == "PlagueStart" ? "50" : "15");
        await UpdateEventRateInDynamoDB("Inflation", selectedEvent == "PlagueStart" ? "20" : "10");
      }
      else if (selectedEvent == "WarStart" || selectedEvent == "WarEnd")
      {
        await UpdateEventRateInDynamoDB("Death", selectedEvent == "WarStart" ? "50" : "10");
        await UpdateEventRateInDynamoDB("Inflation", selectedEvent == "WarStart" ? "20" : "10");
      }

    }

    private string SelectWeightedRandomEvent()
    {
      int totalWeight = eventWeights.Values.Sum();
      int randomValue = random.Next(totalWeight);

      foreach (var kvp in eventWeights)
      {
        if (randomValue < kvp.Value)
        {
          return kvp.Key;
        }
        randomValue -= kvp.Value;
      }
      return events[random.Next(events.Count)]; // Fallback
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

    private async Task CallServiceEndpointsAsync(string eventName, string eventRate, List<long> affectedPeople)
    {
      var requestBody = new
      {
        EventName = eventName,
        EventRate = eventRate,
        AffectedPeople = affectedPeople
      };

      if (eventEndpoints.TryGetValue(eventName, out var endpoints))
      {
        foreach (var endpoint in endpoints)
        {
          var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);

          if (!response.IsSuccessStatusCode)
          {
            throw new Exception($"Failed to call service endpoint {endpoint}. Status code: {response.StatusCode}");
          }
        }
      }
    }

    private async Task UpdateEventRateInDynamoDB(string eventName, string newRate)
    {
      var request = new UpdateItemRequest
      {
        TableName = "EventRates",
        Key = new Dictionary<string, AttributeValue>
        {
            { "EventName", new AttributeValue { S = eventName } }
        },
        AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
        {
            {
                "EventRate",
                new AttributeValueUpdate
                {
                    Action = AttributeAction.PUT,
                    Value = new AttributeValue { S = newRate }
                }
            }
        }
      };
      await dynamoDbClient.UpdateItemAsync(request);
    }

  }
}
