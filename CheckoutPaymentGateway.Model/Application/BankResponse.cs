namespace CheckoutPaymentGateway.Model.Application
{
    using System;

    public class BankResponse
    {
        public Guid TransactionId { get; set; }

        public TransactionStatusCode TransactionStatusCode { get; set; }
    }
}