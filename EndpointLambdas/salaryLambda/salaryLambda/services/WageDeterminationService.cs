using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace salaryLambda.services
{
    public class WageDeterminationService
    {
        private static readonly Random random = new Random();
        private readonly WageService wageService;

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
                int higherWage = minimumWage + random.Next(1, minimumWage / 2 + 1); // Up to 50% higher
                return higherWage;
            }

            return minimumWage;
        }
    }
}
