using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StartOrResetSim.Interfaces;
using System.Text;

namespace StartOrResetSim.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly HttpClient httpClient;

        public RequestHandler()
        {
            httpClient = new HttpClient();
        }

        public async Task<bool> SendPutRequestAsync(string url, bool value, string startTime)
        {
            // Prepare the query parameter based on the boolean value
            string action = value ? "start" : "reset";

            try
            {
                HttpContent content = null;

                var requestBody = new
                {
                    action = action,
                    startTime = value ? startTime : null
                };

                var json = JsonConvert.SerializeObject(requestBody);
                content = new StringContent(json, Encoding.UTF8, "application/json");

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