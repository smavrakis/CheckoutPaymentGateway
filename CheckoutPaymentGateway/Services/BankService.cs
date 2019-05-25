namespace CheckoutPaymentGateway.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Model.Application;

    public class BankService : IBankService
    {
        private readonly ILogger<BankService> _logger;

        public BankService(ILogger<BankService> logger)
        {
            _logger = logger;
        }

        // This is a fake bank service, it will be replaced with a proper one when we're closer to production.
        public async Task<BankResponse> ProcessTransactionAsync(PaymentRequest paymentRequest,
                                                                CancellationToken cancellationToken = default(CancellationToken))
        {
            if (paymentRequest == null)
            {
                throw new ArgumentNullException(nameof(paymentRequest));
            }

            var transactionStatusCode = paymentRequest.Amount <= 2000 ? TransactionStatusCode.Successful : TransactionStatusCode.Unsuccessful;
            var response = new BankResponse { TransactionId = Guid.NewGuid(), TransactionStatusCode = transactionStatusCode };

            _logger.LogTrace("Bank response: {@Response}", response);

            return response;
        }
    }
}