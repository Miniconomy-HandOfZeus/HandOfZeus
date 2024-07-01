using StartOrResetSim.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StartOrResetSim.Services.DeterminePrice;

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
            new Item { Name = "health_insurance", Price = 300 },
            new Item { Name = "prime_lending_rate", Price = 200 },
            new Item { Name = "taxes", Price = 400 },
            new Item { Name = "food_price", Price = 400 },
            new Item { Name = "life_insurance", Price = 400 },
            new Item { Name = "short_term_insurance", Price = 400 },
            new Item { Name = "minimum_wage", Price = 400 },
            new Item { Name = "eletronic_price", Price = 400 },
            new Item { Name = "house_price", Price = 400 }
        };

        public async Task setPrices()
        {
            prices.ForEach(item =>
            {
                DBHelper.SetInDbNumber(item.Name, item.Price.ToString());
            });
        }

        public async Task setStartTime(string key, DateTime startTime)
        {
            DBHelper.SetInDbString(key, startTime.ToString());
        }

    }
}
