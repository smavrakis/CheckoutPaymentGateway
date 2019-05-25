namespace CheckoutPaymentGateway.Tests.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using CheckoutPaymentGateway.Controllers;
    using CheckoutPaymentGateway.Services;
    using Common;
    using Data;
    using Exceptions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Logging;
    using Model.Application;
    using Model.Database;
    using Moq;
    using Xunit;

    public class PaymentsControllerTests : BaseTestFixture, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _appFactory;
        private readonly PaymentsController _paymentsController;
        private readonly Mock<ILogger<PaymentsController>> _loggerMock;
        private readonly Mock<IPaymentsService> _paymentsServiceMock;
        private readonly Mock<IBankService> _bankServiceMock;

        public PaymentsControllerTests(WebApplicationFactory<Startup> appFactory)
        {
            _appFactory = appFactory;

            _loggerMock = new Mock<ILogger<PaymentsController>>();

            _paymentsServiceMock = new Mock<IPaymentsService>();
            _paymentsServiceMock.Setup(p => p.GetPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.FromResult(new Payment { CreditCardNumber = "5847532754938761" }));
            _paymentsServiceMock.Setup(p => p.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.FromResult(new Payment()));
            _paymentsServiceMock.Setup(p => p.MaskCreditCardNumber(It.IsAny<string>())).Returns("************8761");

            _bankServiceMock = new Mock<IBankService>();
            _bankServiceMock.Setup(b => b.ProcessTransactionAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.FromResult(new BankResponse()));

            _paymentsController = new PaymentsController(_loggerMock.Object, _paymentsServiceMock.Object, _bankServiceMock.Object);
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Get_Should_Throw_Exception_If_The_Payment_Is_Not_Found()
        {
            // Arrange
            var paymentsServiceMock = new Mock<IPaymentsService>();
            paymentsServiceMock.Setup(p => p.GetPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.FromResult<Payment>(null));

            var paymentsController = new PaymentsController(_loggerMock.Object, paymentsServiceMock.Object, _bankServiceMock.Object);

            // Act
            async Task HandleRequest() => await paymentsController.Get(Guid.NewGuid(), default(CancellationToken));

            // Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(HandleRequest);
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Get_Should_Call_The_GetPaymentAsync_Method()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            // Act
            await _paymentsController.Get(transactionId, default(CancellationToken));

            // Assert
            _paymentsServiceMock.Verify(p => p.GetPaymentAsync(transactionId, It.IsAny<CancellationToken>()));
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Get_Should_Call_The_MaskCreditCardNumber_Method()
        {
            // Arrange
            // Act
            await _paymentsController.Get(Guid.NewGuid(), default(CancellationToken));

            // Assert
            _paymentsServiceMock.Verify(p => p.MaskCreditCardNumber(It.IsAny<string>()));
        }

        [Fact]
        [Trait(Category, Controllers)]
        [Trait(Category, WebApp)]
        [Trait(Category, LongRunning)]
        public async Task Get_Should_Return_The_Correct_Payment()
        {
            // Arrange
            var dbContext = DbContextFactory.GetPaymentGatewayDbContext();
            var payment = CreateTestPayment(dbContext);

            HttpResponseMessage response;

            // Act
            lock (WebAppLock)
            {
                using (var appFactory = GetTestWebApplicationFactory(_appFactory, dbContext))
                {
                    using (var httpClient = appFactory.CreateClient())
                    {
                        response = httpClient.GetAsync($"{Constants.ApiRoutes.Payments}/{payment.TransactionId}").Result;
                    }
                }
            }

            // Assert
            response.EnsureSuccessStatusCode();
            var paymentToTest = await response.Content.ReadAsAsync<Payment>();
            Assert.Equal(payment.Id, paymentToTest.Id);
            Assert.Equal(payment.TransactionId, paymentToTest.TransactionId);
            Assert.Equal(payment.TransactionStatusCode, paymentToTest.TransactionStatusCode);
            Assert.Equal(payment.Amount, paymentToTest.Amount);
            Assert.Equal(payment.CreditCardCvv, paymentToTest.CreditCardCvv);
            Assert.Equal(payment.CreditCardExpiryMonth, paymentToTest.CreditCardExpiryMonth);
            Assert.Equal(payment.CreditCardExpiryYear, paymentToTest.CreditCardExpiryYear);
            Assert.Equal("************6324", paymentToTest.CreditCardNumber);
            Assert.Equal(payment.Currency, paymentToTest.Currency);
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Post_Should_Throw_Exception_If_The_PaymentRequest_Is_Null()
        {
            // Arrange
            // Act
            async Task HandleRequest() => await _paymentsController.Post(null, default(CancellationToken));

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(HandleRequest);
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Post_Should_Call_The_ProcessTransactionAsync_Method()
        {
            // Arrange
            var paymentRequest = new PaymentRequest();

            // Act
            await _paymentsController.Post(paymentRequest, default(CancellationToken));

            // Assert
            _bankServiceMock.Verify(b => b.ProcessTransactionAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        [Trait(Category, Controllers)]
        public async Task Post_Should_Call_The_CreatePaymentAsync_Method()
        {
            // Arrange
            var paymentRequest = new PaymentRequest();

            // Act
            await _paymentsController.Post(paymentRequest, default(CancellationToken));

            // Assert
            _paymentsServiceMock.Verify(p => p.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        [Trait(Category, Controllers)]
        [Trait(Category, WebApp)]
        [Trait(Category, LongRunning)]
        public async Task Post_Should_Create_The_Payment_Correctly()
        {
            // Arrange
            var dbContext = DbContextFactory.GetPaymentGatewayDbContext();
            var paymentRequest = new PaymentRequest
            {
                Amount = 1500,
                CreditCardNumber = "8584739205746324",
                CreditCardCvv = 567,
                CreditCardExpiryMonth = 12,
                CreditCardExpiryYear = 2020,
                Currency = "USD"
            };

            HttpResponseMessage response;

            // Act
            lock (WebAppLock)
            {
                using (var appFactory = GetTestWebApplicationFactory(_appFactory, dbContext))
                {
                    using (var httpClient = appFactory.CreateClient())
                    {
                        response = httpClient.PostAsJsonAsync(Constants.ApiRoutes.Payments, paymentRequest).Result;
                    }
                }
            }

            // Assert
            response.EnsureSuccessStatusCode();
            var bankResponse = await response.Content.ReadAsAsync<BankResponse>();
            var paymentToTest = dbContext.Payments.Single();
            Assert.Equal(bankResponse.TransactionId, paymentToTest.TransactionId);
            Assert.Equal(bankResponse.TransactionStatusCode, paymentToTest.TransactionStatusCode);
            Assert.Equal(paymentRequest.Amount, paymentToTest.Amount);
            Assert.Equal(paymentRequest.CreditCardCvv, paymentToTest.CreditCardCvv);
            Assert.Equal(paymentRequest.CreditCardExpiryMonth, paymentToTest.CreditCardExpiryMonth);
            Assert.Equal(paymentRequest.CreditCardExpiryYear, paymentToTest.CreditCardExpiryYear);
            Assert.Equal(paymentRequest.CreditCardNumber, paymentToTest.CreditCardNumber);
            Assert.Equal(paymentRequest.Currency, paymentToTest.Currency);
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