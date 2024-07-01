using Amazon;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GettingCertTest;

public class Function
{

    private static readonly string secretName = "Certificate_PFX";
    private readonly IAmazonSecretsManager secretsManagerClient;

    public Function()
    {
        secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1); // Replace with your region
    }

    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        try
        {
            X509Certificate2 cert = await GetCertAndKey();

            if (cert == null)
            {
                return "Error: Certificate and key not retrieved.";
            }

            // Use certAndKey.Cert and certAndKey.Key in your HTTPS request
            // Example: Create HTTPS request with certAndKey.Cert and certAndKey.Key

            LambdaLogger.Log($"Certificate Subject: {cert.Subject}");
            LambdaLogger.Log($"Certificate Thumbprint: {cert.Thumbprint}");

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

            // Create an HttpClient using the handler
            using (var httpClient = new HttpClient(handler))
            {
                var requestUri = "https://api.zeus.projects.bbdgrad.com/food-price"; // Replace with your actual API endpoint
                var content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json"); // Replace with your actual payload

                // Make the PUT request
                var response = await httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                LambdaLogger.Log($"Response: {responseBody}");
                return $"Success: HTTPS request sent. Response: {responseBody}";
            }
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<X509Certificate2> GetCertAndKey()
    {
        //GetSecretValueRequest request = new GetSecretValueRequest
        //{
        //    SecretId = secretName,
        //    VersionStage = "AWSCURRENT",
        //};

        //GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);

        //string secretString = response.SecretString;

        //// Deserialize JSON containing cert and key
        //var certAndKey = Newtonsoft.Json.JsonConvert.DeserializeObject<CertificateSecret>(secretString);

        //return certAndKey;

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT",
        };

        GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);

        var secretObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);

        if (secretObject.TryGetValue("pfx", out string pfxBase64) && secretObject.TryGetValue("password", out string password))
        {
            // Decode base64 string to byte array
            byte[] pfxBytes = Convert.FromBase64String(pfxBase64);

            // Extract certificate and key from PFX with password
            LambdaLogger.Log($"PFX Bytes (Base64): {pfxBase64}");
            LambdaLogger.Log($"Password: {password}");
            X509Certificate2 cert =  new X509Certificate2(pfxBytes, password);
            LambdaLogger.Log($"Certificate Subject: {cert.Subject}");
            LambdaLogger.Log($"Certificate Thumbprint: {cert.Thumbprint}");
            return cert;
        }
        else
        {
            throw new Exception("PFX certificate or password not found in secret.");
        }
    }

    private class CertificateSecret
    {
        public string Key { get; set; }
        public string Cert { get; set; }
    }
}
