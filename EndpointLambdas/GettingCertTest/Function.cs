using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GettingCertTest;

public class Function
{

    private static readonly string secretName = "Certificate";
    private readonly IAmazonSecretsManager secretsManagerClient;
    private readonly AmazonDynamoDBClient client;

    public Function()
    {
        secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1); // Replace with your region
        client = new AmazonDynamoDBClient();
    }

    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = "hand-of-zeus-db",
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "Key", new AttributeValue { S = "food_price" } },
                        { "value", new AttributeValue { N = "200" } }
                    }
            };

            try
            {
                await client.PutItemAsync(request);
                LambdaLogger.Log("Start time updated successfully.");
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Error updating start time: {ex.Message}");
            }

            X509Certificate2 cert = await GetCertAndKey();

            if (cert == null)
            {
                return "Error: Certificate and key not retrieved.";
            }

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);

            foreach (X509Certificate clientCert in handler.ClientCertificates)
            {
                var cert2 = clientCert as X509Certificate2;
                if (cert2 != null)
                {
                    LambdaLogger.Log($"Added Certificate: Subject - {cert2.Subject}, Thumbprint - {cert2.Thumbprint}");
                }
                else
                {
                    LambdaLogger.Log("Added Certificate: Unable to cast to X509Certificate2.");
                }
            }


            //var baseUri = "https://api.zeus.projects.bbdgrad.com/date"; // Replace with your actual API endpoint
            //var timeQueryParam = "1719837257315"; // Use an appropriate format for your time parameter

            //// Construct the full URI with the query parameter
            //var uriBuilder = new UriBuilder(baseUri);
            //var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            //query["time"] = timeQueryParam;
            //uriBuilder.Query = query.ToString();
            //var requestUri = uriBuilder.ToString();

            //// Create an HttpClient using the handler
            //using (var httpClient = new HttpClient(handler))
            //{
            //    //var requestUri = "https://api.zeus.projects.bbdgrad.com/date"; // Replace with your actual API endpoint
            //    //var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json"); // Replace with your actual payload

            //    // Make the PUT request
            //    var response = await httpClient.GetAsync(requestUri);
            //    response.EnsureSuccessStatusCode();

            //    var responseBody = await response.Content.ReadAsStringAsync();
            //    LambdaLogger.Log($"Response: {responseBody}");
            //    return $"Success: HTTPS request sent. Response: {responseBody}";
            //}
            return $"Success: HTTPS request sent. Response:";
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<X509Certificate2> GetCertAndKey()
    {
        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT",
        };

        GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);

        var secretObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);

        var secert = "{ \"pfx\":\"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\", \"password\":\"cert_pass\"}";
        var temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(secert);


        if (temp.TryGetValue("pfx", out string pfxBase64) && temp.TryGetValue("password", out string password))
        {
            // Decode base64 string to byte array
            byte[] pfxBytes = Convert.FromBase64String(pfxBase64);
         
            LambdaLogger.Log($"PFX Bytes (Base64): {pfxBase64}");
            LambdaLogger.Log($"Password: {password}");
            X509Certificate2 cert = new X509Certificate2(pfxBytes, password);
            LambdaLogger.Log($"Certificate Subject: {cert.Subject}");
            LambdaLogger.Log($"Certificate Thumbprint: {cert.Thumbprint}");

            return cert;
        }
        else
        {
            throw new Exception("PFX certificate or password not found in secret.");
        }
    }
}
