﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodPrice.Services
{
    public class GetPriceFromDB
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private static readonly string foodKey = "food_price";
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

            try
            {
                var response = await client.GetItemAsync(request);

                if (response.Item == null || !response.Item.ContainsKey("value"))
                {
                    throw new Exception("Minimum wage not found in the database.");
                }

                return int.Parse(response.Item["value"].N);

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
            
        }

    }
}
