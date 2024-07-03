using Amazon.Lambda.Core;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Amazon;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace StartOrResetSim.Services
{
    public class CertHandler
    {
        private static readonly string secretName = "Certificate";
        private readonly IAmazonSecretsManager secretsManagerClient;

        public CertHandler()
        {
            secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1); // Replace with your region
        }

        public async Task<X509Certificate2> FunctionHandler()
        {
            try
            {
                var certAndKey = await GetCertAndKey();

                if (certAndKey == null)
                {
                    return null;
                }
                return certAndKey;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<X509Certificate2> GetCertAndKey()
        {
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
}
