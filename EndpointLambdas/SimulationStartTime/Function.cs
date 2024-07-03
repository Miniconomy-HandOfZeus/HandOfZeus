using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimulationStartTime;

public class Function
{
    private readonly static string tableName = "hand-of-zeus-db";
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new();

    public static async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var response = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        if (input.QueryStringParameters == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        // Parse the custom query parameters set by API gateway
        if (!input.QueryStringParameters.TryGetValue("allowed_services", out string? allowedServicesString) || !input.QueryStringParameters.TryGetValue("key", out string? key))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        List<string> allowedServices = [.. allowedServicesString.Split(",")];

        context.Logger.Log($"Allowed services: {string.Join(", ", allowedServices)}");
        context.Logger.Log($"DB key: {key}");

        DateTime simulationStartDate = await getSimulationStartDate();

        response.Body = JsonConvert.SerializeObject(new
        {
            start_date = simulationStartDate
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
}
