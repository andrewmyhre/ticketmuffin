using System;
using Moq;
using NUnit.Framework;
using Raven.Client.Embedded;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.PayPal.Model;

namespace GroupGiving.Test.Unit
{
    public class PledgeTestsBase : InMemoryStoreTest
    {
        protected Mock<IEmailRelayService> EmailRelayService = new Mock<IEmailRelayService>();
        protected Mock<IPaymentGateway> PaymentGateway = new Mock<IPaymentGateway>();
        protected Mock<ITaxAmountResolver> TaxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IAccountService> AccountService = new Mock<IAccountService>();
        protected GroupGivingEvent Event;
        protected string PaypalPayKey;
        protected EmbeddableDocumentStore DocumentStore;

        [SetUp]
        public void Setup()
        {
            DocumentStore = InMemoryStore();
        }
        [TearDown]
        public void TearDown()
        {
            DocumentStore.Dispose();
        }

        protected Account ValidAccount()
        {
            return new Account()
                       {
                           PayPalEmail = "testuser@gmail.com"
                       };
        }

        protected void SetTaxRateForCountry(string country, decimal tax)
        {
            TaxResolverMock
                .Setup(m => m.LookupTax(country))
                .Returns(tax);
        }

        protected void PaymentRequestIsSuccessful(string paypalPayKey)
        {
            PaymentGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { payKey = paypalPayKey, PaymentExecStatus = "CREATED"});
        }

        protected void PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest()
        {
            PaymentGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Throws(new HttpChannelException(new FaultMessage() {Error = new PayPalError(){Message="failed"},
                Raw = new DialogueHistoryEntry("request", "response")}));
        }
        protected void PaypalGatewayReturnsAnErrorWhenMakingDelayedPaymentRequest()
        {
            PaymentGateway
                .Setup(m => m.CreateDelayedPayment(It.IsAny<PaymentGatewayRequest>()))
                .Throws(new HttpChannelException(new FaultMessage() { Error = new PayPalError() { Message = "failed" },
                Raw = new DialogueHistoryEntry("request", "response")}));
        }

        protected void PayPalGatewayReturnsTransactionIdWithVerification()
        {
            PaymentGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { payKey = PaypalPayKey, PaymentExecStatus = "CREATED", 
                    DialogueEntry = new DialogueHistoryEntry("request","response")})
                .Verifiable();
        }

        protected void PayPalGatewayReturnsPaymentDetailsForTransactionId(string paymentStatus)
        {
            PaymentGateway
                .Setup(m => m.RetrievePaymentDetails(It.IsAny<string>()))
                .Returns(new PaymentDetailsResponse() {status = paymentStatus, Raw = new DialogueHistoryEntry("request","response")});
        }

        protected void PayPalGatewayCanCreateDelayedPayment(string paypalPayKey="12345")
        {
            PaymentGateway
                .Setup(m => m.CreateDelayedPayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse()
                             {
                                 DialogueEntry = new DialogueHistoryEntry("request","response"),
                                 PaymentExecStatus = "CREATED",
                                 payKey = paypalPayKey
                             });
        }

        protected ISiteConfiguration ValidSiteConfiguration()
        {
            return new SiteConfiguration()
                       {
                           AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration()
                                                               {
                                                                   SandboxMode = true,
                                                                   SandboxApiBaseUrl = "https://svcs.sandbox.paypal.com/",
                                                                   SandboxApplicationId = "APP-80W284485P519543T",
                                                                   DeviceIpAddress = "127.0.0.1",
                                                                   ApiUsername = "platfo_1255077030_biz_api1.gmail.com",
                                                                   ApiPassword = "1255077037",
                                                                   ApiSignature = "Abg0gYcQyxQvnf2HDJkKtA-p6pqhA1k-KTYE0Gcy1diujFio4io5Vqjf",
                                                                   RequestDataFormat = "XML",
                                                                   ResponseDataFormat = "XML",
                                                                   SandboxMailAddress = "something@something.com"
                                                               }
                       };
        }
    }

    public class InMemoryStoreTest
    {
        public EmbeddableDocumentStore InMemoryStore()
        {
            return new EmbeddableDocumentStore { RunInMemory = true }.Initialize() as EmbeddableDocumentStore;
        }
    }
}