using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Numerics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PersonaSacrifice;

public class Function
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Random random = new Random();
    private string url = "https://api.example.com/data"; //Personas endpoint url here\\


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {

        var requestBody = JsonConvert.DeserializeObject<Dictionary<string, int>>(request.Body);
        if (requestBody == null || !requestBody.ContainsKey("number"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonConvert.SerializeObject(new { message = "Invalid request. number of sacrifices is required." }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        int numberOfPotentialSacrifices = requestBody["number"];

        try
        {

            List<BigInteger> peronas = await GetPerson(numberOfPotentialSacrifices);
            List<BigInteger> sacrificedPersonas = SacrificePersonas(peronas);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(new { data = sacrificedPersonas }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (HttpRequestException e)
        {
            context.Logger.LogLine($"Request error: {e.Message}");

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    public async Task<List<BigInteger>> GetPerson(int numberOfPeople)
    {

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        List<BigInteger> bigInts = JsonConvert.DeserializeObject<List<BigInteger>>(responseBody);


        List<BigInteger> persona = GetRandomElements(bigInts, numberOfPeople);

        return persona;
    }

    private List<BigInteger> GetRandomElements(List<BigInteger> list, int count)
    {
        Random random = new Random();
        return list.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private bool DetermineSacrifice()
    {

        return random.NextDouble() < 0.5; //50% chance
    }

    public List<BigInteger> SacrificePersonas(List<BigInteger> persona)
    {
        List<BigInteger> personaList = new List<BigInteger>();

        persona.ForEach(person =>
        {
            bool isSacrificed = DetermineSacrifice();

            if (isSacrificed)
            {
                personaList.Add(person);
            }
        });

        return personaList;
    }

}