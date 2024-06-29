using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StartOrResetSim.Services;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StartOrResetSim;

public class Function
{
    private readonly RequestHandler RequestHandler = new RequestHandler();
    private readonly GetStartTimeFromDB GetStartTimeFromDB = new GetStartTimeFromDB();

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

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Parse the request body to get the person ID
        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, bool>>(request.Body);
        if (requestBody == null || !requestBody.ContainsKey("action"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. 'personaId' is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        bool action = requestBody["action"];

        if (action)
        {
            string startTime = await GetStartTimeFromDB.GetStartTime();
            OtherApiUrls.ForEach(url =>
            {
                RequestHandler.SendPutRequestAsync(url, true, startTime);
            });


        }
        else
        {
            OtherApiUrls.ForEach(url =>
            {
                RequestHandler.SendPutRequestAsync(url, false, "");
            });
        }


        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }


}
