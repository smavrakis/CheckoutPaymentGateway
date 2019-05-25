namespace CheckoutPaymentGateway.Model.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Application;

    public class Payment
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(25)")]
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
        [Column(TypeName = "nvarchar(3)")]
        public string Currency { get; set; }

        [Required]
        public Guid? TransactionId { get; set; }

        [Required]
        public TransactionStatusCode TransactionStatusCode { get; set; }
    }
}