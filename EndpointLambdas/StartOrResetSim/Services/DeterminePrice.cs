using Amazon.Lambda.Core;
using StartOrResetSim.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
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
            new Item { Name = "health_insurance", Price = 100 *1024},
            new Item { Name = "prime_lending_rate", Price = 10 },
            new Item { Name = "food_price", Price = 300*1024 },
            new Item { Name = "life_insurance", Price = 20*1024 },
            new Item { Name = "short_term_insurance", Price = 50*1024 },
            new Item { Name = "minimum_wage", Price = 1500*1024 },
            new Item { Name = "eletronic_price", Price = 400*1024 },
            new Item { Name = "house_price", Price = 50000*1024 }
        };

    public async Task setPrices()
    {
      int business = 10;
      int income = 20;
      int vat = 12;
      DBHelper.setTaxes("taxes", business, income, vat);
      prices.ForEach(item =>
      {
        try
        {
          DBHelper.SetInDbNumber(item.Name, item.Price);
        }
        catch (Exception ex)
        {
          LambdaLogger.Log("there was an error in setting prices: " + ex.Message);
        }

      });
    }

    public async Task setStartTime(string key, string startTime)
    {
        DBHelper.SetInDbString(key, startTime.ToString());
    }

    public async Task setHasStarted(string key, string hasStarted)
    {
        DBHelper.setHasStarted(key, hasStarted);
    }

  }
}
