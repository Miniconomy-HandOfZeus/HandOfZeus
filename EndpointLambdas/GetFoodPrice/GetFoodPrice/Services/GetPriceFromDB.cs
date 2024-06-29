using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFoodPrice.Services
{
    public class GetPriceFromDB
    {
        private static readonly string tableName = "ZeusTable";
        private static readonly string foodKey = "FoodPrice";
        private readonly AmazonDynamoDBClient client;

        public GetPriceFromDB()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<int> GetFoodPrice()
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = foodKey } }
                }
            };

            var response = await client.GetItemAsync(request);
            if (response.Item == null || !response.Item.ContainsKey(foodKey))
            {
                throw new Exception("Minimum wage not found in the database.");
            }

            Console.WriteLine(response.Item[foodKey].N);
            return int.Parse(response.Item[foodKey].N);
        }

    }
}
