namespace CheckoutPaymentGateway.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Model.Database;

    public interface IPaymentsService
    {
        Task<Payment> GetPaymentAsync(Guid transactionId, CancellationToken cancellationToken = default(CancellationToken));

        Task<Payment> CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default(CancellationToken));

        string MaskCreditCardNumber(string creditCardNumber);
    }
}