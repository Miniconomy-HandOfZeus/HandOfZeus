using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MinimumWage
{
    public class Function
    {
        private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
        private static readonly string tableName = "ZeusTable";
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler()
        {
            var task = Function.grabKey();
            System.Console.WriteLine(task.Id + " " + task.ToString() + " " + task.Result);

            return "test";
        }
        public static async Task<string> grabKey()
        {
            var key = new Dictionary<string, AttributeValue>
        {
            { "Key", new AttributeValue { S = "minimum_wage" } }
        };
            GetItemRequest request = new GetItemRequest
            {
                TableName = tableName,
                Key = key
            };
            try
            {
                var response = await _dynamoDbClient.GetItemAsync(request);
                System.Console.WriteLine(response.ToString(), response.Item);
                return response.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
