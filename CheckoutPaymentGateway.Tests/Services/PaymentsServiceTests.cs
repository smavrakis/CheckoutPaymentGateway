namespace CheckoutPaymentGateway.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CheckoutPaymentGateway.Services;
    using Data;
    using Microsoft.Extensions.Logging;
    using Model.Application;
    using Model.Database;
    using Moq;
    using Xunit;

    public class PaymentsServiceTests : BaseTestFixture
    {
        private readonly IPaymentsService _paymentsService;

        public PaymentsServiceTests()
        {
            var loggerMock = new Mock<ILogger<PaymentsService>>();
            _paymentsService = new PaymentsService(loggerMock.Object, DbContextFactory.GetPaymentGatewayDbContext());
        }

        [Fact]
        [Trait(Category, Services)]
        public async Task GetPaymentAsync_Should_Return_The_Payment_Correctly()
        {
            // Arrange
            var dbContext = DbContextFactory.GetPaymentGatewayDbContext();
            var payment = CreateTestPayment(dbContext);

            // Act
            // ReSharper disable once PossibleInvalidOperationException
            var paymentToTest = await _paymentsService.GetPaymentAsync(payment.TransactionId.Value);

            // Assert
            Assert.Equal(payment.Id, paymentToTest.Id);
            Assert.Equal(payment.TransactionId, paymentToTest.TransactionId);
            Assert.Equal(payment.TransactionStatusCode, paymentToTest.TransactionStatusCode);
            Assert.Equal(payment.Amount, paymentToTest.Amount);
            Assert.Equal(payment.CreditCardCvv, paymentToTest.CreditCardCvv);
            Assert.Equal(payment.CreditCardExpiryMonth, paymentToTest.CreditCardExpiryMonth);
            Assert.Equal(payment.CreditCardExpiryYear, paymentToTest.CreditCardExpiryYear);
            Assert.Equal(payment.CreditCardNumber, paymentToTest.CreditCardNumber);
            Assert.Equal(payment.Currency, paymentToTest.Currency);
        }

        [Fact]
        [Trait(Category, Services)]
        public async Task GetPaymentAsync_Should_Return_Null_If_The_Payment_Does_Not_Exist()
        {
            // Arrange
            // Act
            var paymentToTest = await _paymentsService.GetPaymentAsync(Guid.NewGuid());

            // Assert
            Assert.Null(paymentToTest);
        }

        [Fact]
        [Trait(Category, Services)]
        public async Task CreatePaymentAsync_Should_Throw_Exception_If_Payment_Is_Null()
        {
            // Arrange
            // Act
            async Task CreatePayment() => await _paymentsService.CreatePaymentAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(CreatePayment);
        }

        [Fact]
        [Trait(Category, Services)]
        public async Task CreatePaymentAsync_Should_Create_The_Payment_Correctly()
        {
            // Arrange
            var dbContext = DbContextFactory.GetPaymentGatewayDbContext();
            var payment = new Payment
            {
                Amount = 2000,
                CreditCardCvv = 347,
                CreditCardExpiryMonth = 11,
                CreditCardExpiryYear = 2023,
                CreditCardNumber = "9384739205746324",
                Currency = "EUR",
                TransactionId = Guid.NewGuid(),
                TransactionStatusCode = TransactionStatusCode.Successful
            };

            // Act
            await _paymentsService.CreatePaymentAsync(payment);

            // Assert
            var paymentFromDb = dbContext.Payments.Single();
            Assert.Equal(payment.TransactionId, paymentFromDb.TransactionId);
            Assert.Equal(payment.TransactionStatusCode, paymentFromDb.TransactionStatusCode);
            Assert.Equal(payment.Amount, paymentFromDb.Amount);
            Assert.Equal(payment.CreditCardCvv, paymentFromDb.CreditCardCvv);
            Assert.Equal(payment.CreditCardExpiryMonth, paymentFromDb.CreditCardExpiryMonth);
            Assert.Equal(payment.CreditCardExpiryYear, paymentFromDb.CreditCardExpiryYear);
            Assert.Equal(payment.CreditCardNumber, paymentFromDb.CreditCardNumber);
            Assert.Equal(payment.Currency, paymentFromDb.Currency);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [Trait(Category, Services)]
        public void MaskCreditCardNumber_Should_Throw_Exception_If_CreditCardNumber_Is_Null_Or_Empty(string creditCardNumber)
        {
            // Arrange
            // Act
            void CreatePayment() => _paymentsService.MaskCreditCardNumber(creditCardNumber);

            // Assert
            Assert.Throws<ArgumentException>((Action) CreatePayment);
        }

        [Fact]
        [Trait(Category, Services)]
        public void MaskCreditCardNumber_Should_Mask_The_CreditCardNumber_Correctly()
        {
            // Arrange
            const string creditCardNumber = "5847532754938761";

            // Act
            var maskedCreditCardNumber = _paymentsService.MaskCreditCardNumber(creditCardNumber);

            // Assert
            Assert.Equal("************8761", maskedCreditCardNumber);
        }

        private static Payment CreateTestPayment(PaymentGatewayDbContext dbContext)
        {
            var payment = new Payment
            {
                Amount = 1000,
                CreditCardCvv = 345,
                CreditCardExpiryMonth = 11,
                CreditCardExpiryYear = 2022,
                CreditCardNumber = "8584739205746324",
                Currency = "GBP",
                TransactionId = Guid.NewGuid(),
                TransactionStatusCode = TransactionStatusCode.Successful
            };
            dbContext.Payments.Add(payment);

            dbContext.SaveChanges();

            return payment;
        }
    }
}