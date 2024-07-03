using Newtonsoft.Json;
using SickPersonaDies.Models;
using System.Text;

namespace SickPersonaDies.Services
{
    public class RequestHandler
    {
        private readonly HttpClientHandler client;
        private readonly string personasUrl = "https://api.persona.projects.bbdgrad.com/api/HandOfZeus/killPersonas";

        public RequestHandler()
        {
            client = new HttpClientHandler();
        }

        public async Task<bool> SendPostRequestAsync(SickPersonDiesClass survives)
        {

            using (var httpClient = new HttpClient(client))
            {
                try
                {
                    HttpContent content = null;

                    var requestBody = new
                    {
                        personaIds = new List<long> {survives.personID}
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
