namespace CheckoutPaymentGateway.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Model.Application;

    public interface IBankService
    {
        Task<BankResponse> ProcessTransactionAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default(CancellationToken));
    }
}