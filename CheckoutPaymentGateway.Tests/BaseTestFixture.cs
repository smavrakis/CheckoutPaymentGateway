namespace CheckoutPaymentGateway.Tests
{
    using System;
    using Data;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class BaseTestFixture : IDisposable
    {
        // Category names
        protected const string Category = "Category";
        protected const string WebApp = "WebApp";
        protected const string Services = "Services";
        protected const string Controllers = "Controllers";
        protected const string LongRunning = "LongRunning";

        protected static IConfigurationRoot Configuration { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();

        protected TestDbContextFactory DbContextFactory { get; private set; }

        /// <summary>
        /// Use this for tests requiring the web app running, otherwise
        /// there will be concurrency issues since tests are run in parallel.
        /// </summary>
        protected static readonly object WebAppLock = new object();

        private bool _disposed;

        public BaseTestFixture()
        {
            DbContextFactory = new TestDbContextFactory(Configuration);
        }

        protected WebApplicationFactory<TEntryPoint> GetTestWebApplicationFactory<TEntryPoint>(
            WebApplicationFactory<TEntryPoint> appFactory, PaymentGatewayDbContext receiverDbContext)
            where TEntryPoint : class
        {
            return appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(receiverDbContext);
                });
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DbContextFactory?.Dispose();
                DbContextFactory = null;
            }

            _disposed = true;
        }
    }
}