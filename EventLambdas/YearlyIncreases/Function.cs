using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace YearlyIncreases
{
  public class Function
  {


    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";
    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      Function function = new Function();
      function.runTasks();
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(new { message = input.Body }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
      return response;
    }

    private void runTasks()
    {
      int life_ins = generateRate(100, 300)*1024;
      int health_ins = generateRate(200, 500)*1024;
      int short_ins = generateRate(30, 100)*1024;
      int min_wage = generateRate(1500, 2000)*1024;
      int houses = generateRate(30 * min_wage, 40 * min_wage)*1024;
      pushDB("life_insurance", life_ins);
      pushDB("health_insurance", health_ins);
      pushDB("short_term_insurance", short_ins);
      pushDB("minimum_wage", min_wage);
      pushDB("house_price", houses);
      //salaries();

      pushDB("taxes", 0);
    }

    private async void pushDB(string key, int value)
    {
      UpdateItemRequest request = null;
      if (key == "taxes")
      {
        request = increaseTax(key);
      }
      else
      {
        request = new UpdateItemRequest
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
                { ":newval", new AttributeValue { N = value+"" } }
            },
          UpdateExpression = "SET #V = :newval"
        };
      }

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
    private UpdateItemRequest increaseTax(string key)
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
      return request;
    }

    private int generateRate(int upper, int lower)
    {
      Random randomSeed = new Random();
      int seed = randomSeed.Next(int.MinValue, int.MaxValue);
      Random random = new Random(seed);
      int rate = random.Next(upper, lower);
      return rate;
    }
  }

}
