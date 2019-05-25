namespace CheckoutPaymentGateway.Tests
{
    using System;
    using System.Data.Common;
    using Data;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class TestDbContextFactory : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        private DbConnection _dbConnection;
        private PaymentGatewayDbContext _paymentGatewayDbContext;
        private bool _disposed;

        public TestDbContextFactory(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public PaymentGatewayDbContext GetPaymentGatewayDbContext()
        {
            if (_paymentGatewayDbContext != null)
            {
                return _paymentGatewayDbContext;
            }

            if (_dbConnection == null)
            {
                _dbConnection = new SqliteConnection(_configuration.GetConnectionString("SqlLiteConnection"));
                _dbConnection.Open();
            }

            var builder = new DbContextOptionsBuilder<PaymentGatewayDbContext>();
            builder.UseSqlite(_dbConnection);

            _paymentGatewayDbContext = new PaymentGatewayDbContext(builder.Options);
            _paymentGatewayDbContext.Database.EnsureCreated();

            return _paymentGatewayDbContext;
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
                _paymentGatewayDbContext?.Dispose();
                _paymentGatewayDbContext = null;

                // This will automatically delete the database
                _dbConnection?.Dispose();
                _dbConnection = null;
            }

            _disposed = true;
        }
    }
}