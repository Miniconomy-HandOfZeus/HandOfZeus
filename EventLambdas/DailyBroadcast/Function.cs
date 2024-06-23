using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DailyBroadcast;

public class Function
{
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new();
    private static readonly AmazonSQSClient _sqsClient = new();
    private static readonly string tableName = "ZeusTable";
    private static readonly List<string> queueUrls = ["https://sqs.eu-west-1.amazonaws.com/427519652828/SimulationTimeQueue.fifo"]; // TODO: Enter all relevant queues to push the event to

    private static async Task<string> GetCurrentSimulationTime()
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

        // Calculate the current simulation time
        if (response.Item != null && response.Item.TryGetValue("Date", out AttributeValue? value))
        {
            var simulationStartTimeString = value.S;
            var simulationStartTime = DateTime.ParseExact(simulationStartTimeString, "yyyy-MM-ddTHH:mm:ss", null, System.Globalization.DateTimeStyles.None);
            var currentTime = DateTime.Now;

            var currentSimulationTime = DateTime.MinValue.AddSeconds((currentTime - simulationStartTime).TotalSeconds * 720);
            return currentSimulationTime.ToString("yyyy-MM-dd");
        }
        else
        {
            throw new Exception("Failed to get the current simulation time");
        }
    }

    private static async Task SendEventToQueues(string messageBody)
    {
        foreach (var queueUrl in queueUrls)
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageGroupId = "DailyBroadcastGroup"
            };
            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }

    public static async Task<string> FunctionHandler() // This is the function that gets executed every two minutes
    {
        try
        {
            var messageBody = await GetCurrentSimulationTime();
            await SendEventToQueues(messageBody);


            return "Message sent to SQS queues.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
