using Amazon;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GettingCertTest;

public class Function
{

    private static readonly string secretName = "Certification";
    private readonly IAmazonSecretsManager secretsManagerClient;

    public Function()
    {
        secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1); // Replace with your region
    }

    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        try
        {
            var certAndKey = await GetCertAndKey();

            if (certAndKey == null)
            {
                return "Error: Certificate and key not retrieved.";
            }

            // Use certAndKey.Cert and certAndKey.Key in your HTTPS request
            // Example: Create HTTPS request with certAndKey.Cert and certAndKey.Key


            return "Success: HTTPS request sent: " + certAndKey;
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> GetCertAndKey()
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

        LambdaLogger.Log(response.ToString());
        return response.ToString();
    }

    private class CertificateSecret
    {
        public string Key { get; set; }
        public string Cert { get; set; }
    }
}
