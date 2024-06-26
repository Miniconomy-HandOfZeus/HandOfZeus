using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld;

public class Function
{
    /// <summary>
    /// A simple function that returns a "Hello, World!" message.
    /// </summary>
    /// <param name="input">The API Gateway Proxy Request</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns>An API Gateway Proxy Response</returns>
    public static APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var response = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = input.Body }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        return response;
    }
}
