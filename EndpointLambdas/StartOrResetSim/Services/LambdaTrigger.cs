using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Newtonsoft.Json;
using StartOrResetSim.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Services
{
    public class LambdaTrigger : ILambdaTrigger
    {
        private readonly IAmazonLambda _lambdaClient;

        public LambdaTrigger()
        {
            _lambdaClient = new AmazonLambdaClient();
        }

        public async Task<string> InvokeLambdaAsync(string functionName, object payload, ILambdaContext context)
        {
            var invokeRequest = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = payload != null ? JsonConvert.SerializeObject(payload) : null
            };

            var response = await _lambdaClient.InvokeAsync(invokeRequest);

            using (var sr = new StreamReader(response.Payload))
            {
                string responseBody = await sr.ReadToEndAsync();
                context.Logger.LogLine($"Response from {functionName}: {responseBody}");
                return responseBody;
            }
        }

        public async Task<string> InvokeLambdaAsync(string functionName, ILambdaContext context)
        {
            var invokeRequest = new InvokeRequest
            {
                FunctionName = functionName
            };

            var response = await _lambdaClient.InvokeAsync(invokeRequest);

            using (var sr = new StreamReader(response.Payload))
            {
                string responseBody = await sr.ReadToEndAsync();
                context.Logger.LogLine($"Response from {functionName}: {responseBody}");
                return responseBody;
            }
        }

    }
}
