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
            { "FamineStart", new string[] { } }, // No notification for end
            { "FamineEnd", new string[] { } }, // No notification for end
            { "PlagueStart", new string[] { } }, // No notification for end
            { "PlagueEnd", new string[] { } }, // No notification for end
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

    private static readonly Dictionary<string, string> eventDescriptions = new Dictionary<string, string>
        {
            { "Death", "The amount of people that died are: {0}" },
            { "Marriage", "The amount of people that got married are: {0}" },
            { "Birth", "The amount of people that were born are: {0}" },
            { "Hunger", "The amount of people affected by hunger are: {0}" },
            { "Sickness", "The amount of people affected by sickness are: {0}" },
            { "Breakages", "The amount of breakages are: {0}" },
            { "Salary", "The amount of salary increases are: {0}" },
            { "Fired from job", "The amount of people fired from job are: {0}" },
            { "FamineStart", "A famine has started affecting: {0} people" },
            { "FamineEnd", "A famine has ended affecting: {0} people" },
            { "PlagueStart", "A plague has started affecting: {0} people" },
            { "PlagueEnd", "A plague has ended affecting: {0} people" },
            { "WarStart", "A war has started affecting: {0} people" },
            { "WarEnd", "A war has ended affecting: {0} people" },
            { "Apocalypse", "An apocalypse has occurred affecting: {0} people" },
            { "Inflation", "The inflation rate increased affecting: {0} people" }
        };

    public Function()
    {
      dynamoDbClient = new AmazonDynamoDBClient();
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
      var selectedEvent = SelectWeightedRandomEvent();
      context.Logger.LogLine($"Selected Event: {selectedEvent}");

      var eventRate = await GetEventRateAsync(selectedEvent, context);
      if (eventRate == null)
      {
        context.Logger.LogLine($"Event rate for {selectedEvent} is null. Exiting function.");
        return;
      }
      context.Logger.LogLine($"Event Rate: {eventRate}");


      var affectedPeopleCount = GetAffectedPeopleCount(eventRate);
      var affectedPeople = GetRandomPeople(affectedPeopleCount);
      context.Logger.LogLine($"Affected People: {string.Join(", ", affectedPeople)}");

      var description = string.Format(eventDescriptions[selectedEvent], affectedPeopleCount);
      context.Logger.LogLine($"Event Description: {description}");

      var marriagePairs = await CreateMarriagePairs(affectedPeopleCount);

      if (selectedEvent == "Marriage")
      {
        string marriagePairsResult = string.Join(", ", marriagePairs.Select(x => $"({x["firstPerson"]}, {x["secondPerson"]})"));
        await InsertEventIntoDynamoDB(selectedEvent, description, marriagePairsResult);
        await SendMarriagePairsToService(marriagePairs, context);
      } 
      else if (selectedEvent == "Birth" || selectedEvent == "Fired from job" || selectedEvent == "Breakages" || selectedEvent == "Hunger" || selectedEvent == "Sickness") {
        string Affectedresult = string.Join(", ", affectedPeople);
        await InsertEventIntoDynamoDB(selectedEvent, description, Affectedresult);
      }
      else if (selectedEvent == "Salary")
      {
        var salaryIncreases = CalculateSalaryIncreases(affectedPeopleCount);
        string salaryIncreasesResult = string.Join(", ", salaryIncreases.Select(x => $"{x.Key}: {x.Value}%"));
        await InsertEventIntoDynamoDB(selectedEvent, description, salaryIncreasesResult);
        await SendSalaryIncreasesToService(salaryIncreases);
      }
      else
      {
        await InsertEventIntoDynamoDB(selectedEvent, description, affectedPeopleCount.ToString());
      }

      

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
      else if (selectedEvent == "Inflation")
      {
        var newRate = AdjustInflationRate(eventRate);
        await UpdateEventRateInDynamoDB("Inflation", newRate);
      }
      else if (selectedEvent == "Marriage")
      {


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

    private async Task<string> GetEventRateAsync(string eventName, ILambdaContext context)
    {
      var request = new GetItemRequest
      {
        TableName = "EventRates",
        Key = new Dictionary<string, AttributeValue>
                {
                    { "EventName", new AttributeValue { S = eventName } }
                }
      };

      try
      {
        var response = await dynamoDbClient.GetItemAsync(request);

        if (response.Item == null)
        {
          context.Logger.LogLine($"Event rate for {eventName} not found: Item is null");
          return null;
        }

        if (!response.Item.ContainsKey("EventRate"))
        {
          context.Logger.LogLine($"Event rate for {eventName} not found: 'EventRate' key not found");
          return null;
        }

        context.Logger.LogLine($"Event rate for {eventName}: {response.Item["EventRate"].S}");
        return response.Item["EventRate"].S;
      }
      catch (Exception ex)
      {
        context.Logger.LogLine($"Error retrieving event rate for {eventName}: {ex.Message}");
        return null;
      }
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
        return 0;
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

    private string AdjustInflationRate(string currentRate)
    {
      double rate = double.Parse(currentRate);
      double randomPercentage = random.Next(1, 6);
      double newRate = rate * (1 + randomPercentage / 100);
      return newRate.ToString("F2");
    }

    private async Task InsertEventIntoDynamoDB(string eventName, string description, string value)
    {
      string eventKey = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ");

      var request = new PutItemRequest
      {
        TableName = "hand-of-zeus-db",
        Item = new Dictionary<string, AttributeValue>
        {
            { "Key", new AttributeValue { S = eventKey } },
            { "event_name", new AttributeValue { S = eventName } },
            { "type", new AttributeValue { S = "event" } },
            { "description", new AttributeValue { S = description } },
            { "value", new AttributeValue { S = value } },
            { "date", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
        }
      };

      try
      {
        await dynamoDbClient.PutItemAsync(request);
      }
      catch (Exception ex)
      {
        throw new Exception($"Failed to insert event into DynamoDB: {ex.Message}");
      }
    }

    private async Task<List<Dictionary<string, long>>> CreateMarriagePairs(int count)
    {
      var pairs = new List<Dictionary<string, long>>();
      var availablePeople = await FetchCanBeMarriedPeople();

      while (count > 1 && availablePeople.Count > 1)
      {
        int index1 = random.Next(availablePeople.Count);
        long person1 = availablePeople[index1];
        availablePeople.RemoveAt(index1);

        int index2 = random.Next(availablePeople.Count);
        long person2 = availablePeople[index2];
        availablePeople.RemoveAt(index2);

        pairs.Add(new Dictionary<string, long>
        {
            { "firstPerson", person1 },
            { "secondPerson", person2 }
        });
        count -= 2;
      }

      return pairs;
    }


    private async Task SendMarriagePairsToService(List<Dictionary<string, long>> marriagePairs, ILambdaContext context)
    {
      var requestBody = new
      {
        marriagePairs = marriagePairs
      };

      var endpoint = "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/marryPersonas";
      var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);

      context.Logger.LogLine($"requestBody : {requestBody}");
      context.Logger.LogLine($"Response : {response}");

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to send marriage pairs to service endpoint. Status code: {response.StatusCode}");
      }
    }


    private Dictionary<long, double> CalculateSalaryIncreases(int count)
    {
      var salaryIncreases = new Dictionary<long, double>();

      for (int i = 0; i < count; i++)
      {
        long personId = people[random.Next(people.Count)];
        double increase = random.Next(1, 6); // Random increase between 1% and 5%
        salaryIncreases[personId] = increase;
      }

      return salaryIncreases;
    }

    private async Task SendSalaryIncreasesToService(Dictionary<long, double> salaryIncreases)
    {
      var salaryIncreasesPayload = salaryIncreases.Select(increase => new { PersonId = increase.Key, IncreasePercentage = increase.Value }).ToList();

      var response = await httpClient.PostAsJsonAsync("https://labour.projects.bbdgrad.com", salaryIncreasesPayload);

      if (response.IsSuccessStatusCode)
      {
        Console.WriteLine("Salary increases sent successfully.");
      }
      else
      {
        Console.WriteLine($"Failed to send salary increases: {response.StatusCode}");
      }
    }

    private async Task<List<long>> FetchCanBeMarriedPeople()
    {
      var response = await httpClient.GetFromJsonAsync<List<long>>("https://api.persona.projects.bbdgrad.com/api/HandOfZeus/givePersonasChild");
      return response ?? new List<long>();
    }
  }
}
