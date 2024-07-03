using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GetValueFromDB.Services;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetValueFromDB;

public class Function
{
  private readonly Repository db = new();

  public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
  {
    var queryParams = input.QueryStringParameters;
    if (queryParams == null || !queryParams.ContainsKey("service"))
    {
      return new APIGatewayProxyResponse
      {
        StatusCode = 500,
        Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
    }

    context.Logger.Log($"requested the price");
    string key = queryParams["service"];
    int value = await db.GetValue(key);

    return new APIGatewayProxyResponse
    {
      StatusCode = 200,
      Body = JsonConvert.SerializeObject(new { value }),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
  }
}

