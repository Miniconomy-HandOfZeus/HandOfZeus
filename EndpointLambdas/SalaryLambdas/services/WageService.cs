using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalaryLambdas.Interfaces;

namespace SalaryLambdas.services
{
    public class WageService : IWageService
    {
        private static readonly string tableName = "ZeusTable"; 
        private static readonly string minimumWageKey = "minimum_wage";
        private readonly AmazonDynamoDBClient client;

        public WageService()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<int> GetMinimumWageAsync()
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = minimumWageKey } }
                }
            };

            var response = await client.GetItemAsync(request);
            if (response.Item == null || !response.Item.ContainsKey(minimumWageKey))
            {
                throw new Exception("Minimum wage not found in the database.");
            }

            Console.WriteLine(response.Item[minimumWageKey].N);
            return int.Parse(response.Item[minimumWageKey].N);
        }
    }
}
