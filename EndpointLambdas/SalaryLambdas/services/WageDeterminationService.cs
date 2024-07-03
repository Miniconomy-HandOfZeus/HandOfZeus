using SalaryLambdas.Interfaces;

namespace SalaryLambdas.services
{
    public class WageDeterminationService : IWageDetermination
    {
        private readonly static Random random = new Random();
 
        private readonly WageService wageService = new WageService();
        public WageDeterminationService(WageService wageService)
        {
            this.wageService = wageService;
        }

        public async Task<int> DetermineWageAsync()
        {
            int minimumWage = (int)await wageService.GetMinimumWageAsync();
            bool getsHigherWage = random.NextDouble() < 0.5; // 50% chance

            if (getsHigherWage)
            {
                int higherWage = minimumWage + random.Next(1, (minimumWage / 2) + 1); // Up to 50% higher
                return higherWage;
            }

            return minimumWage;
        }
    }
}
