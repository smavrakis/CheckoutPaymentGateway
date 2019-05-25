namespace CheckoutPaymentGateway.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Model.Database;

    public class PaymentsService : IPaymentsService
    {
        private readonly ILogger<PaymentsService> _logger;
        private readonly PaymentGatewayDbContext _dbContext;

        public PaymentsService(ILogger<PaymentsService> logger, PaymentGatewayDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<Payment> GetPaymentAsync(Guid transactionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Retrieving payment with transaction ID {TransactionId}", transactionId);

            var query = _dbContext.Payments.Where(p => p.TransactionId == transactionId);
            var payments = await query.ToListAsync(cancellationToken);

            return payments.FirstOrDefault();
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            // This is just so that the MaskCreditCardNumber method works properly. Needs to be fixed.
            if (payment.CreditCardNumber.Length != 16)
            {
                throw new ArgumentException("Expecting credit card number of length 16.");
            }

            _logger.LogTrace("Creating a new payment: {@Payment}", payment);

            _dbContext.Payments.Add(payment);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return payment;
        }

        public string MaskCreditCardNumber(string creditCardNumber)
        {
            // No time to properly implement this. Of course not all credit card numbers have 16 digits.
            // TODO: Do a proper implementation of this (regex)?
            if (string.IsNullOrWhiteSpace(creditCardNumber) || creditCardNumber.Length != 16)
            {
                throw new ArgumentException("Expecting credit card number of length 16.");
            }

            return $"************{creditCardNumber.Substring(12, 4)}";
        }
    }
}