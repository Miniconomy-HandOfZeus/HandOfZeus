using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StartOrResetSim.Services;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StartOrResetSim;

public class Function
{
    private readonly RequestHandler RequestHandler = new RequestHandler();
    private readonly CertHandler CertHandler = new CertHandler();
    private readonly DeterminePrice DeterminePrice = new DeterminePrice();
    private readonly DBHelper DBHelper = new DBHelper();

    List<string> OtherApiUrls = new List<string> {
        "https://sustenance.projects.bbdgrad.com",
        "https://electronics.projects.bbdgrad.com",
        "https://api.bonds.projects.bbdgrad.com",
        "https://api.loans.projects.bbdgrad.com",
        "https://rentals.projects.bbdgrad.com",
        "https://sales.projects.bbdgrad.com",
        "https://mese.projects.bbdgrad.com",
        "https://labour.projects.bbdgrad.com",
        "https://api.mers.projects.bbdgrad.com",
        "https://care.projects.bbdgrad.com",
        "https://api.insurance.projects.bbdgrad.com",
        "https://api.life.projects.bbdgrad.com",
        "https://api.health.projects.bbdgrad.com",
        "https://api.commercialbank.projects.bbdgrad.com",
        "https://api.retailbank.projects.bbdgrad.com",
        "https://property.projects.bbdgrad.com",
        "https://persona.projects.bbdgrad.com",
    };

    private static readonly string LambdaFunctionName1 = "DateCalculation";
    private static readonly string LambdaFunctionName2 = "RandomEvent";
    private readonly ScheduleTrigger _ScheduleTrigger = new();

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        DateTime currentTime; 
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, bool>>(request.Body);
        if (requestBody == null || !requestBody.ContainsKey("action"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. bool is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        bool action = requestBody["action"];
        X509Certificate2 certs = await CertHandler.GetCertAndKey();

        if (certs != null)
        {
            try
            {
                if (action)
                {
                    currentTime = DateTime.Now;
                    await DeterminePrice.setStartTime("SimulationStartTime", currentTime);
                    await DeterminePrice.setPrices();

                    await _ScheduleTrigger.StartAsync();

                    string startTime = await DBHelper.GetFromDB("SimulationStartTime");
                    OtherApiUrls.ForEach(async url =>
                    {
                        await RequestHandler.SendPutRequestAsync(url, true, startTime, certs);
                    });

                    var body = new
                    {
                        startTime = currentTime
                    };

                    // Serialize the response object to JSON
                    string responseBody = JsonConvert.SerializeObject(body);

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                        Body = responseBody
                    };

                }
                else
                {
                    OtherApiUrls.ForEach(async url =>
                    {
                        await RequestHandler.SendPutRequestAsync(url, false, "", certs);
                    });

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        
                    };
                }
            }
            catch (Exception)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            
        }
        else
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

}
