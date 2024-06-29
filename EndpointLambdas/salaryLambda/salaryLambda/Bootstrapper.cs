using Microsoft.Extensions.DependencyInjection;
using salaryLambda.Interfaces;
using salaryLambda.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace salaryLambda
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IWageDetermination, WageDeterminationService>();
            services.AddScoped<IWageService, WageService>();
            return services;
        }
    }
}
