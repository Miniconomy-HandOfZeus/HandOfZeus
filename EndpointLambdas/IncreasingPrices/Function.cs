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
    //public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    //{
    //  var response = new APIGatewayProxyResponse
    //  {
    //    StatusCode = 200,
    //    Body = JsonSerializer.Serialize(new { message = input.Body }),
    //    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    //  };

    //  Function function = new Function();
    //  string date = "01|01|01";
    //  string ans = function.getRate(date);
    //  return response;
    //}

    public string FunctionHandler(string input, ILambdaContext context)
    {
      string date = "01|12|30";
      processDate(date);
      return input?.ToUpper();
    }

    private void processDate(string date)
    {
      string[] dateSplit = date.Split("|");
      string period = determineCalendarPeriod(dateSplit);
      switch (period){
        case "yearEnd":
          pushDB("life_insurance", generateRate(100, 300));
          pushDB("health_insurance", generateRate(200, 500));
          pushDB("short_term_insurance", generateRate(30, 100));
          pushDB("minimum_wage", generateRate(1500, 2000));
          //housing();
          //salaries();
          pushDB("minimum_wage", generateRate(10, 30));
          goto case "monthEnd";
        case "monthEnd":
          pushDB("food_price", generateRate(300,400));
          pushDB("electronics_price", generateRate(500,750));
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
      //pushDB("tax_rate", random.Next(10, 30) + "");
      return rate;
    }

    private async void pushDB(string key, int value)
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
