namespace CheckoutPaymentGateway.Extensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds payment services to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        public static IServiceCollection AddPayments(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IPaymentsService, PaymentsService>();
            // Will be switched out to the proper implementation when we're closer to production.
            services.AddScoped<IBankService, BankService>();

            return services;
        }
    }
}