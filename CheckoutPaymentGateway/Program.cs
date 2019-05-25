namespace CheckoutPaymentGateway
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            // The default builder is used due to lack of time. To make the app
            // production ready we need to configure it further.
            var builder = WebHost.CreateDefaultBuilder(args);

            builder.ConfigureAppConfiguration(ConfigureAppConfiguration());
            builder.UseSerilog(ConfigureSeriLog());
            builder.UseStartup<Startup>();

            return builder;
        }

        private static Action<WebHostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration()
        {
            return (hostBuilderContext, config) =>
            {
                config.AddJsonFile("appsettings.json", true, true);
                config.AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", true, true);
                // This will be added by the orchestrator (kubernetes?) in production
                config.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
            };
        }

        private static Action<WebHostBuilderContext, LoggerConfiguration> ConfigureSeriLog()
        {
            // TODO: Add logs to database if necessary
            return (context, configuration) =>
            {
                const string consoleOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";
                configuration.ReadFrom.Configuration(context.Configuration);
                configuration.WriteTo.Console(outputTemplate: consoleOutputTemplate);
            };
        }
    }
}