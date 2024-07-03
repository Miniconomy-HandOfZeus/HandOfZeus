using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MontlyIncreases
{
  public class Function
  {
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";
    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      Function function = new Function();
      context.Logger.Log("RUN TASKS");
      function.runTasks();
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(new { message = input.Body }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
      context.Logger.Log(response.ToString());
      return response;
    }
    private void runTasks()
    {
      pushDB("food_price", generateRate(300, 400));
      pushDB("electronics_price", generateRate(500, 750));
      pushDB("prime_lending_rate", generateRate(5, 15));
      checkPopulation();
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

    private async void checkPopulation()
    {
      string key = "population";
      long oldPopulation = 0;
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
        oldPopulation = long.Parse(response.Item["value"].N);
      }
      catch (Exception e)
      {
        throw e;
      }
    }
  }
}
