using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HasSimStarted.Service
{
    public class DBHelper
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private readonly AmazonDynamoDBClient client = new();

        public async Task<bool> GetValue(string key)
        {
            //var request = new GetItemRequest
            //{
            //    TableName = tableName,
            //    Key = new Dictionary<string, AttributeValue>
            //{
            //    { "Key", new AttributeValue { S = key } }
            //}
            //};

            try
            {
                //var response = await client.GetItemAsync(request);

                //if (response.Item == null || !response.Item.TryGetValue("value", out AttributeValue? value))
                //{
                //    throw new Exception($"{key} not found in the db.");
                //}

                //var hasStarted = response.Item["value"].BOOL;



                var request = new GetItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = key } }
                }
                };

                var response = await client.GetItemAsync(request);
                if (response.Item == null || !response.Item.ContainsKey("value"))
                {
                    throw new Exception("Item Not found");
                }

                string hasSatrted = response.Item["value"].S;

                LambdaLogger.Log("REPSONSE FROM DB IS: " + response.Item["value"].S);
                if (hasSatrted == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<string> GetStartTime(string key)
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

                if (response.Item == null || !response.Item.TryGetValue("value", out AttributeValue? value))
                {
                    throw new Exception($"{key} not found in the db.");
                }


                return response.Item["value"].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }

        }
    }
}
