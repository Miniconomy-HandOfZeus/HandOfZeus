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
         "https://property-manager.projects.bbdgrad.com/PropertyManager/reset",
         "https://api.care.projects.bbdgrad.com/api/simulation",
         "https://service.electronics.projects.bbdgrad.com/zeus/control",
         "https://api.mers.projects.bbdgrad.com/api/simulation/startNewSimulation",
         "https://api.health.projects.bbdgrad.com/control-simulation",
         "https://api.life.projects.bbdgrad.com/control-simulation",
         "https://api.insurance.projects.bbdgrad.com/api/time",
         "https://api.loans.projects.bbdgrad.com/mng/reset"

    };

    private readonly ScheduleTrigger _ScheduleTrigger = new();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        string currentTime; 
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
                    currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    await DeterminePrice.setStartTime("SimulationStartTime", currentTime);

                    try
                    {
                        await DeterminePrice.setPrices();
                    }catch (Exception ex)
                    {
                        LambdaLogger.Log("error while setting prices: " + ex.Message);
                    }
                   

                    await _ScheduleTrigger.StartAsync();

                    string startTime = await DBHelper.GetFromDB("SimulationStartTime");

                    OtherApiUrls.ForEach(async url =>
                    {
                        try
                        {
                            await RequestHandler.SendPutRequestAsync(url, true, startTime, certs);
                        }catch (Exception ex)
                        {
                            LambdaLogger.Log($"error while sending request to {url}: " + ex.Message);
                        }


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
                    await _ScheduleTrigger.StopAsync();

                    OtherApiUrls.ForEach(async url =>
                    {
                        try
                        {
                            await RequestHandler.SendPutRequestAsync(url, false, "", certs);
                        }catch (Exception ex)
                        {
                            LambdaLogger.Log($"error while sending request to {url}: " + ex.Message);
                        }


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
