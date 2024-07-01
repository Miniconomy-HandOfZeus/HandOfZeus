using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using StartOrResetSim.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly HttpClient client;
        private const string SecretName = "Certification";
        private const string Region = "eu-west-1";
        private readonly IAmazonSecretsManager _secretsManager;

        public RequestHandler()
        {
            client = new HttpClient(new HttpClientHandler { ClientCertificates = { GetCertificate().Result } });
            _secretsManager = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(Region));
        }

        public async Task<X509Certificate2> GetCertificate()
        {
            var secretValue = await GetSecretAsync(SecretName);

            // Parse the secret (assuming JSON format)
            var secretJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretValue);

            // Load the certificate from the secret
            var cert = new X509Certificate2(Convert.FromBase64String(secretJson["cert"]));
            Console.WriteLine(cert);
            return cert;
        }

        public async Task<string> GetSecretAsync(string secretName)
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
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            Console.WriteLine(response.SecretString.ToString());
            return response.SecretString;
        }

        public async Task<bool> SendPutRequestAsync(string url, bool value, string startTime)
        {
            // Prepare the query parameter based on the boolean value
            string param = value ? "start" : "reset";

            // Build the request URL with the query parameter
            string requestUrl = $"{url}?action={param}";

            try
            {
                HttpContent content = null;

                if (value)
                {
                    var requestBody = new { startTime };
                    var json = JsonConvert.SerializeObject(requestBody);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await client.PutAsync(requestUrl, content);

                response.EnsureSuccessStatusCode();

                return true; // Return true to indicate success
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error sending PUT request: {e.Message}");
                return false; // Return false if there was an error
            }
        }
    }
}
