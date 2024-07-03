using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using SickPersonaDies.Models;
using SickPersonaDies.Services;
using StartOrResetSim.Services;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SickPersonaDie;

public class Function
{
    private static readonly Random random = new Random();
    private static readonly RequestHandler RequestHandler = new RequestHandler();
    private readonly CertHandler CertHandler = new CertHandler();

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, BigInteger>>(input.Body);
        if (requestBody == null || !requestBody.ContainsKey("personaId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'personaId' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        BigInteger personId = requestBody["personaId"];

        // Determine if the person survives or not (50% chance)
        bool survives = random.NextDouble() >= 0.5;

        // Create the response
        sickPersonDiesClass response = new sickPersonDiesClass();
        response.personID = personId;
        response.survives = survives;

        if (!response.survives)
        {
            //X509Certificate2 certs = await CertHandler.GetCertAndKey();
            await RequestHandler.SendPostRequestAsync(response);
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(response),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
