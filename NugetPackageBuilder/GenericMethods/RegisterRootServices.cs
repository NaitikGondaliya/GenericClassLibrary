using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShivOhm.Infrastructure
{
    public static class RegisterRootServices
    {
        public static void RootServicesRegistered(
              this IServiceCollection services)
        {
            services.AddSingleton<ILog, LogNLog>();
        }
    }
}
