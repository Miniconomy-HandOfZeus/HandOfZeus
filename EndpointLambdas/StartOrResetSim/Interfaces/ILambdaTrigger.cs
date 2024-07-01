using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Interfaces
{
    public interface ILambdaTrigger
    {
        public Task<string> InvokeLambdaAsync(string functionName, object payload, ILambdaContext context);
        public Task<string> InvokeLambdaAsync(string functionName, ILambdaContext context);
    }
}
