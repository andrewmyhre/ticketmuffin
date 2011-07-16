using System;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using Moq;

namespace GroupGiving.Test.Unit
{
    public class PledgeTestsBase
    {
        protected Mock<IPaymentGateway> paypalService = new Mock<IPaymentGateway>();
        protected Mock<IRepository<GroupGivingEvent>> eventRepositoryMock = new Mock<IRepository<GroupGivingEvent>>();
        protected Mock<ITaxAmountResolver> taxResolverMock = new Mock<ITaxAmountResolver>();
        protected Mock<IPayPalConfiguration> _paypalConfiguration = new Mock<IPayPalConfiguration>();
        protected GroupGivingEvent _event;
        protected string _paypalPayKey;

        protected void SetDummyPaypalConfiguration()
        {
            _paypalConfiguration.SetupAllProperties();
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
        }

        protected void AnyPayPalRequestReturnsPayKey(string paypalPayKey)
        {
            paypalService
                .Setup(m => m.MakeRequest(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() {TransactionId = paypalPayKey});
        }

        protected void PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest()
        {
            paypalService
                .Setup(m => m.MakeRequest(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { Errors = new[] { new ResponseError() { Message = "failed" } } });
        }

        protected void EventRepositoryStoresEventWithVerification()
        {
            eventRepositoryMock
                .Setup(m => m.SaveOrUpdate(_event))
                .Verifiable();
        }

        protected void PayPalGatewayReturnsTransactionIdWithVerification()
        {
            paypalService
                .Setup(m=>m.MakeRequest(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { TransactionId = _paypalPayKey })
                .Verifiable();
        }
    }
}