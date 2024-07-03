using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

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
            "Sickness",
            //"Breakages",
            //"Salary",
            //"Fired from job",
            "FamineStart",
            "FamineEnd",
            "PlagueStart",
            "PlagueEnd",
            "WarStart",
            "WarEnd",
            "Apocalypse",
            //"Inflation"
        };


    private static readonly Dictionary<string, int> eventWeights = new Dictionary<string, int>
        {
            { "Death", 50 },
            { "Marriage", 15 },
            { "Birth", 75 },
            { "Sickness", 25 },
            //{ "Breakages", 5 },
            //{ "Salary", 25 },
            //{ "Fired from job", 5 },
            { "FamineStart", 5 },
            { "FamineEnd", 0 },
            { "PlagueStart", 5 },
            { "PlagueEnd", 0 },
            { "WarStart", 5 },
            { "WarEnd", 0 },
            { "Apocalypse", 1 },
            //{ "Inflation", 10 }
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

      // Handle start and end events logic
      if (selectedEvent.EndsWith("Start"))
      {
        var endEvent = selectedEvent.Replace("Start", "End");
        eventWeights[endEvent] = 100;
        eventWeights[selectedEvent] = 0;
      }
      else if (selectedEvent.EndsWith("End"))
      {
        eventWeights[selectedEvent] = 0;
        var startEvent = selectedEvent.Replace("End", "Start");
        eventWeights[startEvent] = 2;
      }
      if (selectedEvent == "FamineStart" || selectedEvent == "FamineEnd")
      {
        await UpdateEventRateInDynamoDB("Death", selectedEvent == "FamineStart" ? "40" : "15");
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
      else
      {
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

        if (selectedEvent == "Marriage")
        {
          var marriagePairs = await CreateMarriagePairs(affectedPeopleCount, context);
          string marriagePairsResult = string.Join(", ", marriagePairs.Select(x => $"({x["firstPerson"]}, {x["secondPerson"]})"));
          await InsertEventIntoDynamoDB(selectedEvent, description, marriagePairsResult);
          await SendMarriagePairsToService(marriagePairs, context);
        }
        else if (selectedEvent == "Fired from job" || selectedEvent == "Breakages")
        {
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
        else if (selectedEvent == "Death")
        {
          string Affectedresult = string.Join(", ", affectedPeople);
          await InsertEventIntoDynamoDB(selectedEvent, description, Affectedresult);
          var PeopleCanKill = await FetchCanBeKilled(context);
          await SendKILLSToService(PeopleCanKill, context);
        }
        else if (selectedEvent == "Birth")
        {
          string Affectedresult = string.Join(", ", affectedPeople);
          await InsertEventIntoDynamoDB(selectedEvent, description, Affectedresult);
          var PeopleToGiveBirth = await FetchChildlessPeople(context);
          await SendBirthsToService(PeopleToGiveBirth, context);
        }
        else if (selectedEvent == "Sickness")
        {
          string Affectedresult = string.Join(", ", affectedPeople);
          await InsertEventIntoDynamoDB(selectedEvent, description, Affectedresult);
          var PeopleToMakeSick = await FetchCanGetSick(context);
          await SendSicknessToService(PeopleToMakeSick, context);
        }
        else if (selectedEvent == "Apocalypse")
        {
          string Affectedresult = string.Join(", ", affectedPeople);
          await InsertEventIntoDynamoDB(selectedEvent, description, Affectedresult);
          var PeopleCanKill = await FetchCanBeKilled(context);
          await SendKILLSToService(PeopleCanKill, context);
        }

        else
        {
          await InsertEventIntoDynamoDB(selectedEvent, description, affectedPeopleCount.ToString());
        }
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
        TableName = "hand-of-zeus-events",
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

    private async Task<List<Dictionary<string, long>>> CreateMarriagePairs(int count, ILambdaContext context)
    {
      var pairs = new List<Dictionary<string, long>>();
      var availablePeople = await FetchCanBeMarriedPeople(context);
      context.Logger.LogLine($"availablePeople: {availablePeople}");
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
        context.Logger.LogLine($"pairs Description: {pairs}");

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
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to send marriage pairs to service endpoint. Status code: {response.StatusCode}");
      }
    }

    private async Task SendKILLSToService(List<long> personIDs, ILambdaContext context)
    {
      var requestBody = new
      {
        personaIds = personIDs
      };
      var endpoint = "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/killPersonas";
      var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to send KILLS to service endpoint. Status code: {response.StatusCode}");
      }
    }
    private async Task SendSicknessToService(List<long> personIDs, ILambdaContext context)
    {
      var requestBody = new
      {
        personaIds = personIDs
      };
      var endpoint = "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/givePersonasSickness";
      var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to send SICKNESS to service endpoint. Status code: {response.StatusCode}");
      }
    }
    private async Task SendBirthsToService(List<long> personIDs, ILambdaContext context)
    {
      var requestBody = new
      {
        patentChildIds = personIDs
      };
      var endpoint = "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/givePersonasChild";
      var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to send BIRTHS to service endpoint. Status code: {response.StatusCode}");
      }
    }

    private Dictionary<long, double> CalculateSalaryIncreases(int count)
    {
      var salaryIncreases = new Dictionary<long, double>();

      for (int i = 0; i < count; i++)
      {
        long personId = people[random.Next(people.Count)];
        double increase = random.Next(1, 6);
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

    private async Task<List<long>> FetchCanBeMarriedPeople(ILambdaContext context)
    {
      var response = await httpClient.GetAsync("https://api.persona.projects.bbdgrad.com/api/Persona/getSinglePersonas");
      var responseContent = response.Content.ReadAsStringAsync().Result;;

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to fetch single personas. Status code: {response.StatusCode}, Response: {responseContent}");
      }

      var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);
      var personaIds = new List<long>();
      var data = responseObject.RootElement.GetProperty("data");
      var personaIdsArray = data.GetProperty("personaIds");

      foreach (var id in personaIdsArray.EnumerateArray())
      {
        personaIds.Add(id.GetInt64());
      }

      if (personaIds.Count == 0)
      {
        throw new Exception("No single personas found in the response.");
      }
      return personaIds;
    }

    private async Task<List<long>> FetchCanBeKilled(ILambdaContext context)
    {
      var response = await httpClient.GetAsync("https://api.persona.projects.bbdgrad.com/api/Persona/getAlivePersonasIDs");
      var responseContent = response.Content.ReadAsStringAsync().Result; ;

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to fetch single personas. Status code: {response.StatusCode}, Response: {responseContent}");
      }

      var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);
      var personaIds = new List<long>();
      var data = responseObject.RootElement.GetProperty("data");
      var personaIdsArray = data.GetProperty("personaIds");

      foreach (var id in personaIdsArray.EnumerateArray())
      {
        personaIds.Add(id.GetInt64());
      }

      if (personaIds.Count == 0)
      {
        throw new Exception("No single personas found in the response.");
      }
      return personaIds;
    }

    private async Task<List<long>> FetchCanGetSick(ILambdaContext context)
    {
      var response = await httpClient.GetAsync("https://api.persona.projects.bbdgrad.com/api/Persona/getAlivePersonasIDs");
      var responseContent = response.Content.ReadAsStringAsync().Result; ;

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to fetch personas that can get sick. Status code: {response.StatusCode}, Response: {responseContent}");
      }

      var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);
      var personaIds = new List<long>();
      var data = responseObject.RootElement.GetProperty("data");
      var personaIdsArray = data.GetProperty("personaIds");

      foreach (var id in personaIdsArray.EnumerateArray())
      {
        personaIds.Add(id.GetInt64());
      }

      if (personaIds.Count == 0)
      {
        throw new Exception("No single personas found in the response.");
      }
      return personaIds;
    }

    private async Task<List<long>> FetchChildlessPeople(ILambdaContext context)
    {
      var response = await httpClient.GetAsync("https://api.persona.projects.bbdgrad.com/api/Persona/getChildlessPersonas");
      var responseContent = response.Content.ReadAsStringAsync().Result;
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to fetch childless personas. Status code: {response.StatusCode}, Response: {responseContent}");
      }
      var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);

      var personaIds = new List<long>();
      var data = responseObject.RootElement.GetProperty("data");
      var personaIdsArray = data.GetProperty("personaIds");

      foreach (var id in personaIdsArray.EnumerateArray())
      {
        personaIds.Add(id.GetInt64());
      }

      if (personaIds.Count == 0)
      {
        throw new Exception("No childless personas found in the response.");
      }
      return personaIds;
    }
  }
}
