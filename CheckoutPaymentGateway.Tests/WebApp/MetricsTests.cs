namespace CheckoutPaymentGateway.Tests.WebApp
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class MetricsTests : BaseTestFixture, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _appFactory;

        public MetricsTests(WebApplicationFactory<Startup> appFactory)
        {
            _appFactory = appFactory;
        }

        [Fact]
        [Trait(Category, WebApp)]
        [Trait(Category, LongRunning)]
        public void Basic_Metrics_Should_Be_Available()
        {
            // Arrange
            var dbContext = DbContextFactory.GetPaymentGatewayDbContext();

            HttpResponseMessage response;

            // Act
            lock (WebAppLock)
            {
                using (var appFactory = GetTestWebApplicationFactory(_appFactory, dbContext))
                {
                    using (var httpClient = appFactory.CreateClient())
                    {
                        response = httpClient.GetAsync("/metrics").Result;
                    }
                }
            }

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}