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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly HttpClient client;

        public RequestHandler()
        {
           client = new HttpClient();
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
