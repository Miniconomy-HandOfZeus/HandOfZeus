using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HasSimStarted.Service;
using Newtonsoft.Json;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HasSimStarted;

public class Function
{
    
   private readonly DBHelper dbHelper = new DBHelper();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {

            bool hasStarted = await dbHelper.GetValue("hasStarted");
            LambdaLogger.Log("hasStarted " + hasStarted);

            if (hasStarted)
            {
                DateTime startTime = await dbHelper.getSimulationStartDate();
                LambdaLogger.Log("startTime " + startTime);

                var requestBody = new
                {
                    hasSatrted = hasStarted,
                    startTime = startTime
                };

                var json = JsonConvert.SerializeObject(requestBody);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(new { json }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            else
            {
                var requestBody = new
                {
                    hasSatrted = hasStarted
                };

                var json = JsonConvert.SerializeObject(requestBody);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(new { json }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = System.Text.Json.JsonSerializer.Serialize(ex.Message),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

    }
}
