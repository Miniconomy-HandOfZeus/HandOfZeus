using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MinimumWage
{
  public class Function
  {
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";

    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(new { message = input.Body }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };

      Function function = new Function();
      string date = "01|01|01";
      string ans = function.getWage(date);
      return response;
    }
    private string getWage(string date)
    {
      if (semiAnnual(date))
      {
        generateWage();
      }
      return fetchFromDB("minimum_wage").Result;
    }

    private bool semiAnnual(string date)
    {
      string[] dateSplit = date.Split('|');

      return (dateSplit[1].Equals("01") && dateSplit[2].Equals("01")) || (dateSplit[1].Equals("06") && dateSplit[2].Equals("01"));
    }

    private void generateWage()
    {
      Random randomSeed = new Random();
      int seed = randomSeed.Next(int.MinValue, int.MaxValue);
      Random random = new Random(seed);
      pushDB("minimum_wage", random.Next(10, 30)+"");
      return;
    }
    public static async Task<string> grabKey()
    {
      var key = new Dictionary<string, AttributeValue>
        {
            { "Key", new AttributeValue { S = "minimum_wage" } }
        };
      GetItemRequest request = new GetItemRequest
      {
        TableName = tableName,
        Key = key
      };
      try
      {
        var response = await _dynamoDbClient.GetItemAsync(request);
        System.Console.WriteLine(response.ToString(), response.Item);
        return response.ToString();
      }
      catch
      {
        return null;
      }
    }
    private void pushDB(string key, string value)
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
                { ":newval", new AttributeValue { S = value } }
            },
        UpdateExpression = "SET #V = :newval"
      };
      RequestDB(request);
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

    private async static Task<string> fetchFromDB(string key)
    {
      var dbRequest = new GetItemRequest
      {
        TableName = tableName,
        Key = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = key } }
                }
      };
      try
      {
        var response = await _dynamoDbClient.GetItemAsync(dbRequest);
        if (response.Item == null || !response.Item.ContainsKey("value"))
        {
          throw new Exception("Minimum wage not found in the database.");
        }

        System.Console.WriteLine(response.ToString(), response.Item);
        return response.Item["value"].S;
      }
      catch (Exception e)
      {
        throw e;
      }
      string value = "";
      return value;
    }
  }
}
