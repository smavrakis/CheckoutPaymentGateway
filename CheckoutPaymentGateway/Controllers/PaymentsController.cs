namespace CheckoutPaymentGateway.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Exceptions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model.Application;
    using Model.Database;
    using Prometheus;
    using Services;

    [Route(Constants.ApiRoutes.Payments)]
    [ApiController]
    public class PaymentsController : GatewayControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IPaymentsService _paymentsService;
        private readonly IBankService _bankService;

        public PaymentsController(ILogger<PaymentsController> logger, IPaymentsService paymentsService, IBankService bankService)
        {
            _logger = logger;
            _paymentsService = paymentsService;
            _bankService = bankService;
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<Payment>> Get(Guid transactionId, CancellationToken cancellationToken)
        {
            const string apiRoute = Constants.ApiRoutes.Payments;
            const string httpVerb = Constants.HttpVerbs.Get;

            using (RequestProcessingDurationSummary.WithLabels(apiRoute, httpVerb).NewTimer())
            {
                _logger.LogInformation("Received a request on route {ApiRoute} with http verb {HttpVerb}.", apiRoute, httpVerb);
                RequestsReceivedCounter.WithLabels(apiRoute, httpVerb).Inc();

                var payment = await _paymentsService.GetPaymentAsync(transactionId, cancellationToken);
                if (payment == null)
                {
                    throw new ResourceNotFoundException("Could not find the specified payment.");
                }

                var maskedCreditCardNumber = _paymentsService.MaskCreditCardNumber(payment.CreditCardNumber);
                var response = new Payment
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    CreditCardCvv = payment.CreditCardCvv,
                    CreditCardExpiryMonth = payment.CreditCardExpiryMonth,
                    CreditCardExpiryYear = payment.CreditCardExpiryYear,
                    CreditCardNumber = maskedCreditCardNumber,
                    Currency = payment.Currency,
                    TransactionStatusCode = payment.TransactionStatusCode,
                    TransactionId = payment.TransactionId
                };

                _logger.LogInformation("Successfully processed a request on route {ApiRoute} with http verb {HttpVerb}.", apiRoute, httpVerb);
                RequestsProcessedCounter.WithLabels(apiRoute, httpVerb).Inc();

                return response;
            }
        }

        [HttpPost]
        public async Task<ActionResult<BankResponse>> Post([FromBody] PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            const string apiRoute = Constants.ApiRoutes.Payments;
            const string httpVerb = Constants.HttpVerbs.Post;

            using (RequestProcessingDurationSummary.WithLabels(apiRoute, httpVerb).NewTimer())
            {
                _logger.LogInformation("Received a request on route {ApiRoute} with http verb {HttpVerb}.", apiRoute, httpVerb);
                RequestsReceivedCounter.WithLabels(apiRoute, httpVerb).Inc();

                if (paymentRequest == null)
                {
                    throw new ArgumentNullException(nameof(paymentRequest));
                }

                var bankResponse = await _bankService.ProcessTransactionAsync(paymentRequest, cancellationToken);

                // This is NOT production-ready. From the minimal research I've done on the topic, we're not supposed to store credit card info
                // anywhere, not even encrypted. Due to time constraints and since the database used in this assignment is in-memory only, I've
                // decided to implement it this way to be able to fulfill the assignment's requirements. I've never had to handle credit cards
                // in my professional career so far and if a project requires it in the future, I would do extensive research on the best way
                // to handle them and hopefully ask someone more experienced in the subject for pointers.
                var payment = new Payment
                {
                    Amount = paymentRequest.Amount,
                    CreditCardNumber = paymentRequest.CreditCardNumber,
                    CreditCardCvv = paymentRequest.CreditCardCvv,
                    CreditCardExpiryMonth = paymentRequest.CreditCardExpiryMonth,
                    CreditCardExpiryYear = paymentRequest.CreditCardExpiryYear,
                    Currency = paymentRequest.Currency,
                    TransactionId = bankResponse.TransactionId,
                    TransactionStatusCode = bankResponse.TransactionStatusCode
                };
                await _paymentsService.CreatePaymentAsync(payment, cancellationToken);

                _logger.LogInformation("Successfully processed a request on route {ApiRoute} with http verb {HttpVerb}.", apiRoute, httpVerb);
                RequestsProcessedCounter.WithLabels(apiRoute, httpVerb).Inc();

                return bankResponse;
            }
        }
    }
}