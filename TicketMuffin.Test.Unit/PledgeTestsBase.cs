using System;
using Moq;
using NUnit.Framework;
using Raven.Client.Embedded;
using TicketMuffin.Core;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Configuration;
using TicketMuffin.Web.Conventions;

namespace GroupGiving.Test.Unit
{
    public class PledgeTestsBase : InMemoryStoreTest
    {
        protected Mock<IEmailRelayService> EmailRelayService = new Mock<IEmailRelayService>();
        protected Mock<IPaymentGateway> PaymentGateway = new Mock<IPaymentGateway>();
        protected Mock<ITaxAmountResolver> TaxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IAccountService> AccountService = new Mock<IAccountService>();
        protected Mock<IOrderNumberGenerator> OrderNumberGenerator = new Mock<IOrderNumberGenerator>();
        protected ICurrencyStore _currencyStore = null;
        protected GroupGivingEvent Event;
        protected string PaypalPayKey;
        protected EmbeddableDocumentStore DocumentStore;

        [SetUp]
        public void Setup()
        {
            DocumentStore = InMemoryStore();
            using (var session = DocumentStore.OpenSession())
            {
                _currencyStore = new CurrencyStore(session);
                ((CurrencyStore)_currencyStore).CreateDefaults();
                session.SaveChanges();
            }
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
                           PaymentGatewayId = "testuser@gmail.com"
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
                .Setup(m => m.CreatePayment(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<TicketMuffin.Core.Payments.Receiver[]>()))
                .Returns(new TicketMuffin.Core.Payments.PaymentCreationResponse() { TransactionId = paypalPayKey});
        }

        protected void PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest()
        {
            PaymentGateway
                .Setup(m => m.CreatePayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TicketMuffin.Core.Payments.Receiver[]>()))
                .Throws(new Exception("Something bad happened"));
        }
        protected void PaymentGatewayReturnsAnErrorWhenMakingDelayedPaymentRequest()
        {
            PaymentGateway
                .Setup(m => m.CreateDelayedPayment(It.IsAny<PaymentGatewayRequest>()))
                .Throws(new HttpChannelException(new FaultMessage() { Error = new PayPalError() { Message = "failed" },
                Raw = new DialogueHistoryEntry("request", "response")}));
        }

        protected void PayPalGatewayReturnsTransactionIdWithVerification()
        {
            PaymentGateway
                .Setup(m => m.CreatePayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TicketMuffin.Core.Payments.Receiver[]>()))
                .Returns(new PaymentCreationResponse()
                {
                    TransactionId = PaypalPayKey
                })
                .Verifiable();
        }

        protected void PaymentGatewayReturnsPaymentDetailsForTransactionId(PaymentStatus paymentStatus)
        {
            PaymentGateway
                .Setup(m => m.RetrievePaymentDetails(It.IsAny<string>()))
                .Returns(new TicketMuffin.Core.Payments.PaymentDetailsResponse() {PaymentStatus = paymentStatus});
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

        protected void PayPalWillAuthoriseACharge(string paypalPayKey)
        {
            PaymentGateway
                .Setup(m => m.AuthoriseCharge(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new PaymentAuthoriseResponse()
                    {
                        Diagnostics = new TransactionDiagnostics(),
                        RedirectUrl = "redirect url",
                        Status = PaymentStatus.Unauthorised,
                        Successful=true,
                        TransactionId = paypalPayKey
                    })
                .Verifiable();
        }
    }

    public class InMemoryStoreTest
    {
        public EmbeddableDocumentStore InMemoryStore()
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true }.Initialize() as EmbeddableDocumentStore;
            store.Conventions.RegisterIdConvention<LocalisedContent>((dbname, commands, content) => String.Join("/", "content", content.Culture, content.Address, content.Label));
            store.Conventions.RegisterIdConvention<Currency>((dbname, commands, currency) => "currencies/" + currency.Iso4217NumericCode);
            return store;
        }
    }
}