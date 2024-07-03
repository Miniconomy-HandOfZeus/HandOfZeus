using Newtonsoft.Json;
using SickPersonaDies.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SickPersonaDies.Services
{
    public class RequestHandler
    {
        private readonly HttpClientHandler client;
        private readonly string personasUrl = "https://persona.projects.bbdgrad.com/killPersonas";

        public RequestHandler()
        {
            client = new HttpClientHandler();
        }

        public async Task<bool> SendPostRequestAsync(X509Certificate2 cert, sickPersonDiesClass survives)
        {
            client.ClientCertificates.Add(cert);

            using (var httpClient = new HttpClient(client))
            {
                try
                {
                    HttpContent content = null;

                    var requestBody = new
                    {
                        personaId = survives.personID
                    };

                    var json = JsonConvert.SerializeObject(requestBody);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    

                    var response = await httpClient.PostAsync(personasUrl, content);

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
