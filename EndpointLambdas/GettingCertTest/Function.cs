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

    private const string SecretName = "Certification";
    private const string Region = "eu-west-1";

    private readonly IAmazonSecretsManager _secretsManager;

    public Function()
    {
        _secretsManager = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(Region));
    }

    public async Task<X509Certificate2> GetCertificateAsync()
    {
        var secretValue = await GetSecretAsync(SecretName);

        // Parse the secret JSON (assuming JSON format)
        var secretJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretValue);

        // Load the certificate from the secret (assuming it is base64 encoded)
        var cert = new X509Certificate2(Convert.FromBase64String(secretJson["cert"]));
        return cert;
    }

    private async Task<string> GetSecretAsync(string secretName)
    {
        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse response;

        try
        {
            response = await _secretsManager.GetSecretValueAsync(request);
        }
        catch (Exception e)
        {
            // Handle exceptions (logging, rethrowing, etc.)
            throw e;
        }

        return response.SecretString;
    }

    public async Task<string> FunctionHandler(ILambdaContext context)
    {
        try
        {
            var certificate = await GetCertificateAsync();
            LambdaLogger.Log("Certificate retrieved successfully");

            // Log certificate details for debugging purposes (optional)
            LambdaLogger.Log($"Certificate Subject: {certificate.Subject}");
            LambdaLogger.Log($"Certificate Issuer: {certificate.Issuer}");

            return "Certificate retrieved successfully";
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error retrieving certificate: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
