namespace CheckoutPaymentGateway.Model.Application
{
    using System.ComponentModel.DataAnnotations;

    public class PaymentRequest
    {
        // TODO: Add proper validation besides the required attribute

        [Required]
        public string CreditCardNumber { get; set; }

        [Required]
        public int? CreditCardExpiryMonth { get; set; }

        [Required]
        public int? CreditCardExpiryYear { get; set; }

        [Required]
        public int? CreditCardCvv { get; set; }

        [Required]
        public decimal? Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }
    }
}