using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;


namespace JancsiVisionLogServers
{
    public static class ConsoleLogExtensions
    {
        public static void AddConsoleLog(this IServiceCollection services)
        {
            services.AddScoped<ConsoleLogProvider>();
        }

    }
}
