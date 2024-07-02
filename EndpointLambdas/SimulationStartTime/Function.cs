using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimulationStartTime;

public class Function
{
    private readonly static List<string> allowedServices = ["persona", "property", "retail_bank", "commercial_bank", "health_insurance", "life_insurance", "short_term_insurance", "health_care", "central_revenue", "labour", "stock_exchange", "real_estate_sales", "real_estate_agent", "short_term_lender", "home_loans", "electronics_retailer", "food_retailer", "zeus"];
    private readonly static string tableName = "hand-of-zeus-db";
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new();

    public static async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var response = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        // Validate the calling service
        if (input.RequestContext.Authorizer.TryGetValue("clientCertCN", out var callingServiceObject))
        {
            string? callingService = callingServiceObject?.ToString();
            if (callingService == null)
            {
                response.StatusCode = 403;
                response.Body = JsonSerializer.Serialize(new
                {
                    message = "Forbidden"
                });
                return response;
            }
            context.Logger.Log($"{callingService} requested the simulation start time");
            if (!allowedServices.Contains(callingService))
            {
                response.StatusCode = 403;
                response.Body = JsonSerializer.Serialize(new
                {
                    message = "Forbidden"
                });
                return response;
            }
        }
        else
        {
            response.StatusCode = 403;
            response.Body = JsonSerializer.Serialize(new
            {
                message = "Forbidden"
            });
            return response;
        }

        DateTime simulationStartDate = await getSimulationStartDate();

        response.Body = JsonSerializer.Serialize(new
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
