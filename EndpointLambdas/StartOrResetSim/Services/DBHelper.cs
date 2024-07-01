using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Services
{
    public class DBHelper
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private static readonly string foodKey = "SimulationStartTime";

        private readonly AmazonDynamoDBClient client;

        public DBHelper()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<string> GetFromDB(string key)
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

                if (response.Item == null || !response.Item.ContainsKey("Value"))
                {
                    throw new Exception("Minimum wage not found in the database.");
                }

                return response.Item["Value"].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }

        }

        public async Task SetStartTime(string key, string value)
        {
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "Key", new AttributeValue { S = key } }, 
                        { "Value", new AttributeValue { S = value } }
                    }
                };

                try
                {
                    await client.PutItemAsync(request);
                    Console.WriteLine("Start time updated successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating start time: {ex.Message}");
                }
        }
    }
}
