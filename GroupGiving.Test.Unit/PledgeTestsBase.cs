using System;
using GroupGiving.Core;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Email;
using GroupGiving.Core.PayPal;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using Moq;
using Raven.Client;
using PaymentDetailsResponse = GroupGiving.Core.Dto.PaymentDetailsResponse;

namespace GroupGiving.Test.Unit
{
    public class PledgeTestsBase
    {
        protected Mock<IEmailRelayService> EmailRelayService = new Mock<IEmailRelayService>();
        protected Mock<IPaymentGateway> PaymentGateway = new Mock<IPaymentGateway>();
        protected Mock<IRepository<GroupGivingEvent>> EventRepositoryMock = new Mock<IRepository<GroupGivingEvent>>();
        protected Mock<ITaxAmountResolver> TaxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IDocumentStore> DocumentStore = new Mock<IDocumentStore>();
        protected Mock<IDocumentSession> DocumentSession = new Mock<IDocumentSession>();
        protected Mock<IAccountService> AccountService = new Mock<IAccountService>();
        protected GroupGivingEvent Event;
        protected string PaypalPayKey;

        protected void SetUpDocumentStore()
        {
            DocumentStore
                .Setup(m => m.OpenSession())
                .Returns(DocumentSession.Object);
        }

        protected void SessionLoadsAccount(Account account)
        {
            DocumentSession
                .Setup(m => m.Load<Account>(It.IsAny<string>()))
                .Returns(account);
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

        protected void EventRepositoryReturns(GroupGivingEvent @event)
        {
            DocumentSession
                .Setup(m => m.Load<GroupGivingEvent>(It.IsAny<string>()))
                .Returns(@event);

            EventRepositoryMock
                .Setup(m => m.Retrieve(It.IsAny<Func<GroupGivingEvent, bool>>()))
                .Returns(@event);
            EventRepositoryMock
                .Setup(m => m.Query(It.IsAny<Func<GroupGivingEvent, bool>>()))
                .Returns(new []{@event});
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

        protected void EventRepositoryStoresEventWithVerification()
        {
            EventRepositoryMock
                .Setup(m => m.SaveOrUpdate(Event))
                .Verifiable();
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
                .Setup(m => m.RetrievePaymentDetails(It.IsAny<GroupGiving.Core.Dto.PaymentDetailsRequest>()))
                .Returns(new PaymentDetailsResponse()
                             {
                                 DialogueEntry = new DialogueHistoryEntry("request","response"),
                                 Status = paymentStatus
                             });
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
}