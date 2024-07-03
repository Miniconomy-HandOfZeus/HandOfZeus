using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace StartOrResetSim.Services
{
    public class DBHelper
    {
        private static readonly string tableName = "hand-of-zeus-events";

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

                if (response.Item == null || !response.Item.ContainsKey("value"))
                {
                    throw new Exception($"cant get {key} from db");
                }

                return response.Item["value"].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }

        }

        public async Task SetInDbString(string key, string value)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "Key", new AttributeValue { S = key } },
                        { "value", new AttributeValue { S = value + ""} }
                    }
            };

            try
            {
                await client.PutItemAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating {key}: {ex.Message}");
            }
        }

        public async Task SetInDbNumber(string key, int value)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = key } },
                    { "value", new AttributeValue { N = value + "" } }
                }
            };

            try
            {
                await client.PutItemAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating {key}: {ex.Message}");
            }
        }

        public async Task setTaxes(string key, int business, int income, int vat)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "Key", new AttributeValue { S = key } },
                    { "business", new AttributeValue { N = business + "" } },
                    { "income", new AttributeValue { N = income + "" } },
                    { "vat", new AttributeValue { N = vat + "" } }
                }
            };
            try
            {
                await client.PutItemAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating {key}: {ex.Message}");
            }
        }
    }
}
