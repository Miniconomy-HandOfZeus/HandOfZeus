using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using SalaryLambdas.Models;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SalaryLambdas;

public class Function
{
    //private readonly WageDeterminationService wageDeterminationService = new WageDeterminationService(new WageService());
    private readonly AmazonDynamoDBClient client = new();
    private static readonly string tableName = "hand-of-zeus-db";
    private static readonly string minimumWageKey = "minimum_wage";
    private readonly static Random random = new Random();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            if (input.QueryStringParameters == null)
            {
                throw new Exception("Query string parameters are null");
            }

            // Parse the input body to get the people
            var requestBody = JsonConvert.DeserializeObject<Dictionary<string, List<long>>>(input.Body);
            if (requestBody == null || !requestBody.ContainsKey("people"))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'people' is required." }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            List<long> personIds = requestBody["people"];

            // Get the minimum wage from the db
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = minimumWageKey } }
                }
            };

            var response = await client.GetItemAsync(request);
            if (response.Item == null || !response.Item.ContainsKey("value"))
            {
                throw new Exception("Minimum wage not found in the database.");
            }

            int minimumWage = int.Parse(response.Item["value"].N);

            // Determine wages
            List<PersonaWages> personaWages = personIds.Select(id => random.NextDouble() < 0.5 ? new PersonaWages { personaId = id, wage = minimumWage } : new PersonaWages { personaId = id, wage = minimumWage + random.Next(minimumWage / 2) }).ToList();

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(personaWages),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: ${ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        
    }
}
