using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace HealthInsurance.Services
{
    public class GetPriceFromDB
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private static readonly string key = "health_insurance";
        private readonly AmazonDynamoDBClient client;

        public GetPriceFromDB()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<int> GetHealthInsurancePrice()
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } }
            }
            };

            try
            {
                var response = await client.GetItemAsync(request);

                if (response.Item == null || !response.Item.ContainsKey("value"))
                {
                    throw new Exception("Health insurance price not found in the db.");
                }

                return int.Parse(response.Item["value"].N);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

        }

    }
}