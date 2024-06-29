using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using salaryLambda.Interfaces;

namespace salaryLambda.services
{
    public class WageService : IWageService
    {
        private static readonly string tableName = "ZeusTable"; 
        private static readonly string minimumWageKey = "MinimumWage";
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
            if (response.Item == null || !response.Item.ContainsKey("Amount"))
            {
                throw new Exception("Minimum wage not found in the database.");
            }

            return int.Parse(response.Item["Amount"].N);
        }
    }
}
