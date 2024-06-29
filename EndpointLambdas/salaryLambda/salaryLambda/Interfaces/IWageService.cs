using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace salaryLambda.Interfaces
{
    public interface IWageService
    {
        public Task<int> GetMinimumWageAsync();
    }
}
