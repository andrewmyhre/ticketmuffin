using System;
using GroupGiving.Core;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using Moq;
using Raven.Client;

namespace GroupGiving.Test.Unit
{
    public class PledgeTestsBase
    {
        protected Mock<IEmailRelayService> EmailRelayService = new Mock<IEmailRelayService>();
        protected Mock<IPaymentGateway> PaymentGateway = new Mock<IPaymentGateway>();
        protected Mock<IRepository<GroupGivingEvent>> EventRepositoryMock = new Mock<IRepository<GroupGivingEvent>>();
        protected Mock<ITaxAmountResolver> TaxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IPayPalConfiguration> PaypalConfiguration = new Mock<IPayPalConfiguration>();
        protected Mock<IDocumentStore> DocumentStore = new Mock<IDocumentStore>();
        protected Mock<IDocumentSession> DocumentSession = new Mock<IDocumentSession>();
        protected Mock<IAccountService> AccountService = new Mock<IAccountService>();
        protected GroupGivingEvent Event;
        protected string PaypalPayKey;

        protected void SetDummyPaypalConfiguration()
        {
            PaypalConfiguration.SetupAllProperties();
        }

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
                .Throws(new HttpChannelException(new FaultMessage() {Error = new PayPalError(){Message="failed"}}));
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
                .Returns(new PaymentGatewayResponse() { payKey = PaypalPayKey, PaymentExecStatus = "CREATED"})
                .Verifiable();
        }
    }
}