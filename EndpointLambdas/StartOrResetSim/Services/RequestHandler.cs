using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using StartOrResetSim.Interfaces;
using StartOrResetSim.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly HttpClientHandler client;

        public RequestHandler()
        {
           client = new HttpClientHandler();
        }

        public async Task<bool> SendPutRequestAsync(string url, bool value, string startTime, X509Certificate2 cert)
        {
            client.ClientCertificates.Add(cert);
            // Prepare the query parameter based on the boolean value
            string action = value ? "start" : "reset";

            using (var httpClient = new HttpClient(client))
            {
                try
                {
                    HttpContent content = null;

                    if (value)
                    {
                        var requestBody = new
                        {
                            startTime = startTime,
                            action = action
                        };

                        var json = JsonConvert.SerializeObject(requestBody);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    var response = await httpClient.PutAsync(url, content);

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
}
