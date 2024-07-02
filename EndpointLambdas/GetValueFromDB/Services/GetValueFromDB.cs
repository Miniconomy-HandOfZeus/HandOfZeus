using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace GetValueFromDB.Services
{
    public class Repository
    {
        private static readonly string tableName = "hand-of-zeus-db";
        private readonly AmazonDynamoDBClient client = new();

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

            try
            {
                var response = await client.GetItemAsync(request);

                if (response.Item == null || !response.Item.TryGetValue("value", out AttributeValue? value))
                {
                    throw new Exception($"{key} not found in the db.");
                }

                return int.Parse(value.N);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

        }

    }
}