using Amazon.Lambda.Core;
using StartOrResetSim.Interfaces;

namespace StartOrResetSim.Services
{
    public class DeterminePrice : IDeterminePrice
    {
        private readonly DBHelper DBHelper = new DBHelper();
        public class Item
        {
            public string Name { get; set; }
            public int Price { get; set; }
        }

        List<Item> prices = new List<Item>
{
            new Item { Name = "health_insurance", Price = 100 },
            new Item { Name = "prime_lending_rate", Price = 100 },
            new Item { Name = "taxes", Price = 400 },
            new Item { Name = "food_price", Price = 400 },
            new Item { Name = "life_insurance", Price = 400 },
            new Item { Name = "short_term_insurance", Price = 400 },
            new Item { Name = "minimum_wage", Price = 600 },
            new Item { Name = "eletronic_price", Price = 400 },
            new Item { Name = "house_price", Price = 400 }
        };

        public async Task setPrices()
        {
            prices.ForEach(item =>
            {
                try
                {
                    DBHelper.SetInDbNumber(item.Name, item.Price);
                }catch (Exception ex)
                {
                    LambdaLogger.Log("there was an error in setting prices: " +  ex.Message);
                }
                
            });
        }

        public async Task setStartTime(string key, string startTime)
        {
            DBHelper.SetInDbString(key, startTime.ToString());
        }

    }
}
