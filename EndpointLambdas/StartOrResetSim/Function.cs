using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StartOrResetSim.Services;
using System.Security.Cryptography.X509Certificates;
using static StartOrResetSim.Services.DeterminePrice;
using static System.Net.WebRequestMethods;

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
         "https://api.loans.projects.bbdgrad.com/mng/reset",
         "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/startSimulation",
         "https://api.commercialbank.projects.bbdgrad.com/simulation/setup",
         "https://api.rentals.projects.bbdgrad.com/api/zeus"
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

    try
    {
      if (action)
      {
        currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        LambdaLogger.Log("the start time is: " + currentTime);
        await DeterminePrice.setStartTime("SimulationStartTime", currentTime);
        await DeterminePrice.setHasStarted("hasStarted", "true");
        try
        {
          await DeterminePrice.setPrices();
        }
        catch (Exception ex)
        {
          LambdaLogger.Log("error while setting prices: " + ex.Message);
        }


        await _ScheduleTrigger.StartAsync();

        string startTime = await DBHelper.GetFromDB("SimulationStartTime");

        OtherApiUrls.ForEach(async url =>
        {
          try
          {
            await RequestHandler.SendPutRequestAsync(url, true, currentTime);
          }
          catch (Exception ex)
          {
            LambdaLogger.Log($"error while sending request to {url}: " + ex.Message);
          }


        });

        var json = new
        {
          startTime = currentTime
        };

        // Serialize the response object to JSON

        //var json = JsonConvert.SerializeObject(body);

        return new APIGatewayProxyResponse
        {
          StatusCode = 200,
          Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
          Body = JsonConvert.SerializeObject(new { json }),

        };

      }
      else
      {
        await _ScheduleTrigger.StopAsync();
        await DeterminePrice.setHasStarted("hasStarted", "false");
        context.Logger.Log("clear DB");
        clearDB();
        context.Logger.Log("Fnish clear, broadcasting reset");
        OtherApiUrls.ForEach(async url =>
                {
                  try
                  {
                    await RequestHandler.SendPutRequestAsync(url, false, "");
                  }
                  catch (Exception ex)
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

  private async Task clearDB()
  {
    //start clear events db

    var scanResponse = DBHelper.scanDB().Result;

    if (scanResponse.Items.Count == 0)
    {
      Console.WriteLine("Table is already empty.");
      return;
    }

    // Iterate over each item and delete it
    foreach (var item in scanResponse.Items)
    {
      await DBHelper.deleteFromDB(item);
    }
  }
}
