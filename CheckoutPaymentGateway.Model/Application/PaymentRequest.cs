namespace CheckoutPaymentGateway.Model.Application
{
    using System.ComponentModel.DataAnnotations;

    public class PaymentRequest
    {
        // TODO: Add more validation? Currency ISO codes etc...

        [Required]
        [CreditCard]
        public string CreditCardNumber { get; set; }

        [Required]
        [Range(1, 12)]
        public int? CreditCardExpiryMonth { get; set; }

        [Required]
        [Range(0, 99)]
        public int? CreditCardExpiryYear { get; set; }

        [Required]
        [Range(100, 9999)]
        public int? CreditCardCvv { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal? Amount { get; set; }

        [Required]
        [StringLength(3, ErrorMessage = "Please use the currency's ISO code.", MinimumLength = 3)]
        public string Currency { get; set; }
    }
}