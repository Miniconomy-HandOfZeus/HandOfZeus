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

namespace TaxRate
{
  public class Function
  {
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private static readonly string tableName = "hand-of-zeus-db";

    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      string[] rate = new Function().getRate();
      try
      {
        var response = new APIGatewayProxyResponse
        {
          StatusCode = 200,
          Body = JsonSerializer.Serialize(new { business = rate[0], income = rate[1], vat = rate[2] }),
          Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
        return response;
      }
      catch (Exception e)
      {
        var response = new APIGatewayProxyResponse
        {
          StatusCode = 200,
          Body = JsonSerializer.Serialize(new { message = "Internal Server Error"}),
          Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
        return response;
      }
    }

    private string[] getRate()
    {
      return fetchFromDB("taxes").Result;
    }

    private async static Task<string[]> fetchFromDB(string key)
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
        string[] arr = { response.Item["business"].N, response.Item["income"].N, response.Item["vat"].N };
        return arr;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
  }
}
