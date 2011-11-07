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
        protected Mock<IEmailRelayService> _emailFacade = new Mock<IEmailRelayService>();
        protected Mock<IPaymentGateway> paypalGateway = new Mock<IPaymentGateway>();
        protected Mock<IRepository<GroupGivingEvent>> eventRepositoryMock = new Mock<IRepository<GroupGivingEvent>>();
        protected Mock<ITaxAmountResolver> taxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IPayPalConfiguration> _paypalConfiguration = new Mock<IPayPalConfiguration>();
        protected Mock<IDocumentStore> _documentStore = new Mock<IDocumentStore>();
        protected Mock<IDocumentSession> _documentSession = new Mock<IDocumentSession>();
        protected GroupGivingEvent _event;
        protected string _paypalPayKey;

        protected void SetDummyPaypalConfiguration()
        {
            _paypalConfiguration.SetupAllProperties();
        }

        protected void SetUpDocumentStore()
        {
            _documentStore
                .Setup(m => m.OpenSession())
                .Returns(_documentSession.Object);
        }

        protected void SessionLoadsAccount(Account account)
        {
            _documentSession
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
            taxResolverMock
                .Setup(m => m.LookupTax(country))
                .Returns(tax);
        }

        protected void EventRepositoryReturns(GroupGivingEvent @event)
        {
            eventRepositoryMock
                .Setup(m => m.Retrieve(It.IsAny<Func<GroupGivingEvent, bool>>()))
                .Returns(@event);
            eventRepositoryMock
                .Setup(m => m.Query(It.IsAny<Func<GroupGivingEvent, bool>>()))
                .Returns(new []{@event});
        }

        protected void PaymentRequestIsSuccessful(string paypalPayKey)
        {
            paypalGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { payKey = paypalPayKey, PaymentExecStatus = "CREATED"});
        }

        protected void PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest()
        {
            paypalGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Throws(new HttpChannelException(new FaultMessage() {Error = new PayPalError(){Message="failed"}}));
        }

        protected void EventRepositoryStoresEventWithVerification()
        {
            eventRepositoryMock
                .Setup(m => m.SaveOrUpdate(_event))
                .Verifiable();
        }

        protected void PayPalGatewayReturnsTransactionIdWithVerification()
        {
            paypalGateway
                .Setup(m => m.CreatePayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { payKey = _paypalPayKey, PaymentExecStatus = "CREATED"})
                .Verifiable();
        }
    }
}