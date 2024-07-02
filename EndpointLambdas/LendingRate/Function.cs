using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LendingRate
{
  public class Function
  {
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";

    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      Function function = new Function(); 
      string lendingRate = function.getRate("");
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(new { rate = lendingRate }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };

      return response;
    }

    private string getRate(string date)
    {
      //if (MonthEnd(date))
      //{
      //  generateRate();
      //}
      return fetchFromDB("prime_lending_rate").Result;
    }
    private Boolean MonthEnd(string date)
    {
      string[] dateSplit = date.Split('|');

      return dateSplit[2].Equals("01");
    }
    private void generateRate()
    {
      Random randomSeed = new Random();
      int seed = randomSeed.Next(int.MinValue, int.MaxValue);
      Random random = new Random(seed);
      pushDB("prime_lending_rate", random.Next(5, 15)+"");
      return;
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
        System.Console.WriteLine(response.ToString(), response.Item);
        return response.Item["value"].S;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
  }
}
