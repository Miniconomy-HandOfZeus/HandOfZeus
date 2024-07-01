using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using StartOrResetSim.Models;
using StartOrResetSim.Services;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StartOrResetSim;

public class Function
{
    private readonly RequestHandler RequestHandler = new RequestHandler();
    private readonly GetStartTimeFromDB GetStartTimeFromDB = new GetStartTimeFromDB();
    private readonly CertHandler CertHandler = new CertHandler();

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
    private readonly LambdaTrigger _LambdaTrigger;

    public Function()
    {
        _LambdaTrigger = new LambdaTrigger();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        
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
        CertClass certs = await CertHandler.GetCertAndKey();

        //if (certs != null) {
        //    if (action)
        //    {
        //        await _LambdaTrigger.InvokeLambdaAsync(LambdaFunctionName1, context);
        //        await _LambdaTrigger.InvokeLambdaAsync(LambdaFunctionName2, context);

        //        string startTime = await GetStartTimeFromDB.GetStartTime();
        //        OtherApiUrls.ForEach(url =>
        //        {
        //            RequestHandler.SendPutRequestAsync(url, true, startTime);
        //        });


        //    }
        //    else
        //    {
        //        OtherApiUrls.ForEach(url =>
        //        {
        //            RequestHandler.SendPutRequestAsync(url, false, "");
        //        });
        //    }
        //}


        // Create HttpClientHandler with client certificate
        var handler = new HttpClientHandler();

        // Convert certificate and key to X509Certificate2
        var certificate = new X509Certificate2(Convert.FromBase64String(certs.Cert),
                                               certs.Key);

        // Add the certificate to the handler
        handler.ClientCertificates.Add(certificate);
        foreach (var cert in handler.ClientCertificates)
        {
            LambdaLogger.Log($"Certificate Subject: {cert.Subject}");
            LambdaLogger.Log($"Certificate Thumbprint: {cert.GetCertHashString()}");
            // Add more properties as needed to verify the certificate
        }

        // Create HttpClient with handler
        //using (var client = new HttpClient(handler))
        //{
        //    // Example PUT request
        //    var apiUrl = "https://example.com/api/resource"; // Replace with your API endpoint
        //    var content = new StringContent("{ \"key\": \"value\" }", Encoding.UTF8, "application/json");

        //    var response = await client.PutAsync(apiUrl, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        LambdaLogger.Log("Success: HTTPS request sent.");
        //        return new APIGatewayProxyResponse
        //        {
        //            StatusCode = 200,
        //            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //        };

        //    }
        //    else
        //    {
        //        LambdaLogger.Log($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        //        return new APIGatewayProxyResponse
        //        {
        //            StatusCode = 500,
        //            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //        };
        //    }
        //}

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };



    }

}
