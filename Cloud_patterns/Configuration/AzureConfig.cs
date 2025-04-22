using System;
using Microsoft.Extensions.Configuration;

namespace Cloud_patterns.Configuration
{
    public static class AzureConfig
    {
        private static readonly Lazy<IConfiguration> _lazyConfig = new Lazy<IConfiguration>(() =>
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        });

        public static IConfiguration Instance => _lazyConfig.Value;

        public static string ServiceBusConnectionString => 
            Instance["Azure:ServiceBus:ConnectionString"] ?? 
            throw new InvalidOperationException("Service Bus connection string not found in configuration");
    }
}