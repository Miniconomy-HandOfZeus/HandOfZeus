using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using salaryLambda.services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace salaryLambda;

public class Function
{
    private readonly WageDeterminationService wageDeterminationService;

    public Function()
    {
        var wageService = new WageService();
        wageDeterminationService = new WageDeterminationService(wageService);
    }

    public async Task<int> FunctionHandler(ILambdaContext context)
    {
        return await wageDeterminationService.DetermineWageAsync();
    }
}
