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
            X509Certificate2 certAndKey = await GetCertAndKey();

            if (certAndKey == null)
            {
                return "Error: Certificate and key not retrieved.";
            }

            // Use certAndKey.Cert and certAndKey.Key in your HTTPS request
            // Example: Create HTTPS request with certAndKey.Cert and certAndKey.Key

            LambdaLogger.Log($"Certificate Subject: {certAndKey.Subject}");
            LambdaLogger.Log($"Certificate Thumbprint: {certAndKey.Thumbprint}");
            return "Success: HTTPS request sent: " + certAndKey;
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
