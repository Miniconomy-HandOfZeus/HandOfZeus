using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StartOrResetSim.Interfaces;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StartOrResetSim.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly HttpClientHandler client;

        public RequestHandler()
        {
           client = new HttpClientHandler();
        }

        public async Task<bool> SendPutRequestAsync(string url, bool value, string startTime)
        {
            //client.ClientCertificates.Add(cert);
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
                            action = action,
                            startTime = startTime
                        };

                        var json = JsonConvert.SerializeObject(requestBody);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                    else
                    {
                        var requestBody = new
                        {
                            action = action,
                        };

                        var json = JsonConvert.SerializeObject(requestBody);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    var response = await httpClient.PostAsync(url, content);
                    LambdaLogger.Log(response.ToString());

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
