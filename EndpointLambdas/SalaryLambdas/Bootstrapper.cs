using Microsoft.Extensions.DependencyInjection;
using SalaryLambdas.Interfaces;
using SalaryLambdas.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalaryLambdas
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
