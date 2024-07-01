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
    private static readonly string region = "eu-west-1";
    private readonly IAmazonSecretsManager secretsManagerClient;

    public Function()
    {
        secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
    }

    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        try
        {
            string secretString = await GetSecret();
            LambdaLogger.Log($"Secret retrieved successfully: {secretString}");

            // Process the secret as needed (e.g., decode base-64, parse JSON)
            var secretObject = JsonConvert.DeserializeObject<CertificateSecret>(secretString);

            // Example: Decode the certificate and key from base-64
            byte[] certificateBytes = Convert.FromBase64String(secretObject.Certificate);
            byte[] keyBytes = Convert.FromBase64String(secretObject.Key);

            // Use the certificate and key bytes as needed
            // Example: var certificate = new X509Certificate2(certificateBytes);

            return "Certificate retrieved successfully";
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error retrieving certificate: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> GetSecret()
    {
        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT",
        };

        GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);

        return response.SecretString;
    }

    private class CertificateSecret
    {
        public string Certificate { get; set; }
        public string Key { get; set; }
    }
}
