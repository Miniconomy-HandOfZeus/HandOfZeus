using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace EletronicPrice.Services
{
    public class GetValueFromDB
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private readonly AmazonDynamoDBClient client;

        public GetValueFromDB()
        {
            client = new AmazonDynamoDBClient();
        }

        public async Task<int> GetValue(string key)
        {
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
                throw new Exception("Item not found");
            }

            Console.WriteLine(response.Item["value"].N);
            return int.Parse(response.Item["value"].N);
        }

    }
}
