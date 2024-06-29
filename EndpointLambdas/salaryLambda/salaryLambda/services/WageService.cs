using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace salaryLambda.services
{
    public class WageService
    {
        private static readonly string tableName = "Wages";
        private static readonly string minimumWageKey = "MinimumWage";
        private readonly AmazonDynamoDBClient client;

        public WageService()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<decimal> GetMinimumWageAsync()
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "WageType", new AttributeValue { S = minimumWageKey } }
                }
            };

            var response = await client.GetItemAsync(request);
            if (response.Item == null || !response.Item.ContainsKey("Amount"))
            {
                throw new Exception("Minimum wage not found in the database.");
            }

            return decimal.Parse(response.Item["Amount"].N);
        }
    }
}
