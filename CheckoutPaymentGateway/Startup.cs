namespace CheckoutPaymentGateway
{
    using System;
    using Data;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Middleware;
    using Prometheus;

    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IConfiguration _configuration;
        private SqliteConnection _sqliteConnection;
        private PaymentGatewayDbContext _paymentGatewayDbContext;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                _logger.LogInformation("Added MVC to services.");

                services.AddPayments();
                _logger.LogInformation("Added Payments to services.");

                // Entity framework core was chosen as the ORM because it boasts great performance in the majority of the cases.
                // In cases where performance is critical and EF core can't cope, we can always just use raw SQL commands through it.
                // Also, it implements the repository pattern and the unit of work pattern so we don't have to handle that ourselves.
                // Which means that we can switch the database provider whenever we want (as long as EF Core supports it).
                // For the scope of this assignment, I've chosen to just use SqlLite in memory. That would of course never be used in
                // production, but it can be switched to SQL Server or other providers effortlessly and the app will just work with no further code changes.

                // This is the code that would normally be used to add the db context to the services (using a different provider would be very similar).
                // services.AddDbContext<PaymentGatewayDbContext>(options => options.UseSqlite(_configuration.GetConnectionString("SqlLiteConnection")));

                // Unfortunately, this doesn't work with SqlLite in-memory since the asp.net core framework adds the db context as a scoped dependency,
                // which means that for each request it spins up a new instance of the class. Which results in the sql connection closing and the database
                // disappearing (SqlLite in-memory retains the data only as long as the connection remains open). So we have to get a bit creative here and
                // do an ugly hack to get this to work. I'm manually creating the db context and injecting it as a singleton. This will keep the connection
                // open until the app closes. This also means that we can't work with migrations like we normally would.
                _sqliteConnection = new SqliteConnection(_configuration.GetConnectionString("SqlLiteConnection"));
                _sqliteConnection.Open();
                var builder = new DbContextOptionsBuilder<PaymentGatewayDbContext>();
                builder.UseSqlite(_sqliteConnection);
                _paymentGatewayDbContext = new PaymentGatewayDbContext(builder.Options);
                _paymentGatewayDbContext.Database.EnsureCreated();
                services.AddSingleton(_paymentGatewayDbContext);
                _logger.LogInformation("Added DB context to services.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Error when seting up the gateway.");
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            try
            {
                applicationLifetime.ApplicationStopped.Register(OnShutdown);

                if (!env.IsDevelopment())
                {
                    // We can safely remove this and https redirection if we're planning to add a reverse proxy
                    // with tls/ssl termination in front of this gateway.
                    app.UseHsts();
                    _logger.LogInformation("Using HSTS middleware.");
                }

                app.UseHttpsRedirection();
                _logger.LogInformation("Using HTTPS redirection middleware.");

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
                _logger.LogInformation("Using forwarded headers middleware.");

                app.UseMiddleware<ExceptionHandlingMiddleware>();
                _logger.LogInformation("Using exception handling middleware.");

                app.UseMetricServer();
                _logger.LogInformation("Using metrics exporter middleware.");

                app.UseMvc();
                _logger.LogInformation("Using MVC middleware.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Error when seting up the gateway.");
                throw;
            }
        }

        private void OnShutdown()
        {
            // Deletes the in-memory database
            _paymentGatewayDbContext?.Dispose();
            _sqliteConnection?.Dispose();
        }
    }
}