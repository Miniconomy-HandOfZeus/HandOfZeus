using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StartOrResetSim.Interfaces
{
    public interface IRequestHandler
    {
        public Task<bool> SendPutRequestAsync(string url, bool value, string startTime);
    }
}
