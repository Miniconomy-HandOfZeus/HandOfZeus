using Amazon.Lambda.Core;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using StartOrResetSim.Models;
using Amazon.Runtime.Internal;

namespace StartOrResetSim.Services
{
    public class CertHandler
    {
        private static readonly string secretName = "Certification";
        private readonly IAmazonSecretsManager secretsManagerClient;

        public CertHandler()
        {
            secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1); // Replace with your region
        }

        public async Task<CertClass> FunctionHandler()
        {
            try
            {
                var certAndKey = await GetCertAndKey();

                if (certAndKey == null)
                {
                    return null;
                }

                // Use certAndKey.Cert and certAndKey.Key in your HTTPS request
                // Example: Create HTTPS request with certAndKey.Cert and certAndKey.Key

                LambdaLogger.Log("KEY: " + certAndKey.Key);
                LambdaLogger.Log("CERT: " + certAndKey.Cert);
                return certAndKey;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<CertClass> GetCertAndKey()
        {
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT",
            };

            GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);

            string secretString = response.SecretString;

            // Deserialize JSON containing cert and key
            var certAndKey = Newtonsoft.Json.JsonConvert.DeserializeObject<CertClass>(secretString);

            return certAndKey;
        }

        //private class CertificateSecret
        //{
        //    public string Key { get; set; }
        //    public string Cert { get; set; }
        //}
    }
}
