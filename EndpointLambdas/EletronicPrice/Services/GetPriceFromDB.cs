using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EletronicPrice.Services
{
    public class GetPriceFromDB
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private static readonly string eletronicKey = "eletronic_price";
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
                    { "Key", new AttributeValue { S = eletronicKey } }
                }
            };

            var response = await client.GetItemAsync(request);

            if (response.Item == null || !response.Item.ContainsKey(eletronicKey))
            {
                throw new Exception("Itme not found " + response?.Item.ToString());
            }

            Console.WriteLine(response.Item[eletronicKey].N);
            return int.Parse(response.Item[eletronicKey].N);
        }

    }
}
