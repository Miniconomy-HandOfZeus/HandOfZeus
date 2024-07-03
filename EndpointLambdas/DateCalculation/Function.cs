using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DateCalculation
{
  public class Function
  {
    private readonly static string tableName = "hand-of-zeus-db";
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new();

    public static async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
      ////var headers = request.Headers;
      var response = new APIGatewayProxyResponse
      {
        StatusCode = 200,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };

      long systemTimeMilliseconds;

      if (input.QueryStringParameters == null)
      {
        response.StatusCode = 400;
        response.Body = JsonConvert.SerializeObject(new
        {
          message = "Missing required query parameter 'time'"
        });
        return response;
      }

      if (input.QueryStringParameters.TryGetValue("time", out string systemTimeMillisecondsString))
      {
        if (long.TryParse(systemTimeMillisecondsString, out systemTimeMilliseconds))
        {
          if (systemTimeMilliseconds < 0)
          {
            response.StatusCode = 400;
            response.Body = JsonConvert.SerializeObject(new
            {
              message = "Query parameter 'time' must be a positive integer"
            });
            return response;
          }
        }
        else
        {
          response.StatusCode = 400;
          response.Body = JsonConvert.SerializeObject(new
          {
            message = "The 'time' query parameter must be an integer"
          });
          return response;
        }
      }
      else
      {
        response.StatusCode = 400;
        response.Body = JsonConvert.SerializeObject(new
        {
          message = "Missing required query parameter 'time'"
        });
        return response;
      }

      DateTime simulationStartDate = await getSimulationStartDate();
      DateTime currentDate = DateTimeOffset.FromUnixTimeMilliseconds(systemTimeMilliseconds).LocalDateTime;

      if (currentDate < simulationStartDate)
      {
        response.StatusCode = 400;
        response.Body = JsonConvert.SerializeObject(new
        {
          message = "The time sent must be greater than the simulation start time"
        });
        return response;
      }

      string date = calculateDate(simulationStartDate, currentDate);
      response.Body = JsonConvert.SerializeObject(new
      {
        date
      });

      return response;
    }

    private static async Task<DateTime> getSimulationStartDate()
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

      if (response.Item != null && response.Item.TryGetValue("value", out AttributeValue? value))
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

    private static string calculateDate(DateTime simulationStartDate, DateTime currentDate)
    {
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
  }
}

