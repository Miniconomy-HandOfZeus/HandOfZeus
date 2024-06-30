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

namespace LifeInsurance
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
      string ans = function.getInsurance(date);
      return response;
    }

    private string getInsurance(string date)
    {
      if (YearEnd(date))
      {
        generateRate();
      }
      return fetchFromDB("life_insurance").Result;
    }

    private void generateRate()
    {
      Random randomSeed = new Random();
      int seed = randomSeed.Next(int.MinValue, int.MaxValue);
      Random random = new Random(seed);
      pushDB("life_insurance", random.Next());
      return;
    }

    private Boolean YearEnd(string date)
    {
      string[] dateSplit = date.Split('|');

      return dateSplit[1].Equals("01") && dateSplit[2].Equals("01");
    }


    private void pushDB(string key, object value)
    {
      Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
        {
            { "Key", new AttributeValue { S = key } },
            { "Value", new AttributeValue { S = JsonSerializer.Serialize(new { value = value }) } }
        };

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
        return response.Item[key].S;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
  }
}

