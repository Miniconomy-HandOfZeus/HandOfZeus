using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace IncreasingPrices
{
  public class Function
  {

    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";
    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      Function function = new Function();
      string date = function.calculateDate();
      function.processDate("");
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(new { message = input.Body }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
      return response;
    }
    private string calculateDate()
    {
      long timeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      DateTime currentDate = DateTimeOffset.FromUnixTimeMilliseconds(timeMS).LocalDateTime;
      DateTime simulationStartDate = getSimulationStartDate().Result;
      //private static string calculateDate(DateTime simulationStartDate, DateTime currentDate)

      // Get the current day of the simulation (eg: day 1302)
      int simulationDayNumber = (int)Math.Floor(((currentDate - simulationStartDate).TotalSeconds / 120) + 1);

      // Calculate current year
      int year = (int)Math.Floor((double)(simulationDayNumber / 360)) + 1;
      int daysIntoYear = (int)Math.Floor((double)(simulationDayNumber % 360));

      // Calculate current month and day
      int month = (int)Math.Floor((double)(daysIntoYear / 30)) + 1;
      int day = (int)Math.Floor((double)daysIntoYear % 30);

      // Format the year, month, and day with leading zeros
      string formattedDate = $"{year:00}|{month:00}|{day:00}";

      return formattedDate;
    }

    private async Task<DateTime> getSimulationStartDate()
    {
      // Get real-world simulation start time from DB
      var key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = "SimulationStartTime" } }
            };
      var request = new GetItemRequest
      {
        TableName = tableName,
        Key = key
      };
      var response = await _dynamoDbClient.GetItemAsync(request);

      if (response.Item != null && response.Item.TryGetValue("Value", out AttributeValue? value))
      {
        string simulationStartTimeString = value.S;
        DateTime simulationStartTime = DateTime.ParseExact(simulationStartTimeString, "yyyy-MM-ddTHH:mm:ss", null, System.Globalization.DateTimeStyles.None);

        return simulationStartTime;
      }
      else
      {
        throw new Exception("Unable to retrieve the simulation start time from the db");
      }
    }

    private void processDate(string date)
    {
      string[] dateSplit = date.Split("|");
      string period = determineCalendarPeriod(dateSplit);
      switch (period)
      {
        case "yearEnd":
          pushDB("life_insurance", generateRate(100, 300));
          pushDB("health_insurance", generateRate(200, 500));
          pushDB("short_term_insurance", generateRate(30, 100));
          pushDB("minimum_wage", generateRate(1500, 2000));
          //housing();
          //salaries();

          pushDB("taxes", 0);
          goto case "monthEnd";
        case "monthEnd":
          pushDB("food_price", generateRate(300, 400));
          pushDB("electronics_price", generateRate(500, 750));
          pushDB("prime_lending_rate", generateRate(5, 15));
          break;
      }

    }
    private int generateRate(int upper, int lower)
    {
      Random randomSeed = new Random();
      int seed = randomSeed.Next(int.MinValue, int.MaxValue);
      Random random = new Random(seed);
      int rate = random.Next(upper, lower);
      return rate;
    }

    private async void pushDB(string key, int value)
    {
      if (key == "taxes")
      {
        increaseTax(key);
      }
      else
      {
        var request = new UpdateItemRequest
        {
          TableName = tableName,
          Key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } }
            },
          ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#V", "value" }
            },
          ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":newval", new AttributeValue { S = value+"" } }
            },
          UpdateExpression = "SET #V = :newval"
        };
        await RequestDB(request);
      }
    }
    private async void increaseTax(string key)
    {
      var taxes = new Dictionary<string, int>
        {
            { "business", generateRate(15,25)},
            { "income", generateRate(20,30) },
            { "vat", generateRate(10,18)}
        };
      var updateExpression = new List<string>();
      var expressionAttributeValues = new Dictionary<string, AttributeValue>();

      foreach (var t in taxes)
      {
        updateExpression.Add($"{t.Key} = :{t.Key}");
        expressionAttributeValues[$":{t.Key}"] = new AttributeValue
        {
          S = t.Value.ToString()
        };
      }

      var request = new UpdateItemRequest
      {
        TableName = tableName,
        Key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } }
            },
        UpdateExpression = "SET " + string.Join(", ", updateExpression),
        ExpressionAttributeValues = expressionAttributeValues
      };

      await RequestDB(request);

    }
    private async Task<UpdateItemResponse> RequestDB(UpdateItemRequest request)
    {
      try
      {
        var response = await _dynamoDbClient.UpdateItemAsync(request);
        Console.WriteLine("Update succeeded.");
        return response;
      }
      catch (Exception e)
      {
        Console.WriteLine("Update failed. Exception: " + e.Message);
        throw e;
      }
    }

    private string determineCalendarPeriod(string[] dateSplit)
    {
      if (yearEnd(dateSplit))
      {
        return "yearEnd";
      }
      else if (monthEnd(dateSplit))
      {
        return "monthEnd";
      }
      return "";
    }
    private bool yearEnd(string[] dateSplit)
    {
      return dateSplit[1].Equals("12") && dateSplit[2].Equals("30");
    }

    //private bool semiAnnual(string[] dateSplit)
    //{
    //  return (dateSplit[1].Equals("06") || dateSplit[1].Equals("12") && dateSplit[2].Equals("30"));
    //}
    private bool monthEnd(string[] dateSplit)
    {
      return dateSplit[2].Equals("30");
    }
  }
}
