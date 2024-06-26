using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace salaryLambda;

public class Function
{
    
    private static readonly AmazonDynamoDBClient _dynamoDbClient = new();
    private static readonly string tableName = "ZeusTable";
    private static readonly Table _table = Table.LoadTable(_dynamoDbClient, tableName);

    public static async Task<float> GetSalary()
    {
        float minimiumWage = await GetMiniumWage();
        int randomNumber = GenerateRandomNumber(0,4);
        float salary;

        switch (randomNumber)
        {
            case 0:
                return minimiumWage;
                break;
            case 1:
                salary = (float)CalculatePercentageIncrease(10, minimiumWage);
                return salary;
                break;
            case 2:
                salary = (float)CalculatePercentageIncrease(20, minimiumWage);
                return salary;
                break;
            case 3:
                salary = (float)CalculatePercentageIncrease(30, minimiumWage);
                return salary;
                break;
            default:
                return minimiumWage;
                break;
        }


    }

    private static async Task<float> GetMiniumWage()
    {
        float miniumWage = await _table.GetItemAsync();
        return miniumWage;
    }

    private static int GenerateRandomNumber(int min, int max)
    {
        Random random = new Random();
        return random.Next(min, max); // The upper bound is exclusive, so use 4 to include 3.
    }

    static double CalculatePercentageIncrease(double percentage, float minimumWage)
    {
        double increase = (minimumWage * percentage) / 100;
        return minimumWage + increase;
    }
}
