using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using NUnit.Framework;

namespace GroupGiving.Test.Unit
{
    [TestFixture]
    public class MakingAPaymentGatewayRequest : PaymentGatewayTestsBase
    {
        protected PayPal.PayPalPaymentGateway _gateway=null;
        
        [SetUp]
        public void Setup()
        {
            _gateway = new PayPalPaymentGateway(null,ValidPayPalConfiguration().AdaptiveAccountsConfiguration);
        }

        [Test]
        public void ASuccessCallbackUrlIsRequired()
        {
            PaymentGatewayRequest request=ValidPaymentGatewayRequest();
            request.SuccessCallbackUrl = "";

            Assert.Throws<ArgumentException>(() => _gateway.CreatePayment(request), "Success callback url is required");
        }

        [Test]
        public void AFailureUrlIsRequired()
        {
            PaymentGatewayRequest request = ValidPaymentGatewayRequest();
            request.FailureCallbackUrl = "";

            Assert.Throws<ArgumentException>(() => _gateway.CreatePayment(request), "Failure callback url is required");
        }

        [Test]
        public void AOrderMemoIsRequired()
        {
            var request = ValidPaymentGatewayRequest();
            request.OrderMemo = "";

            Assert.Throws<ArgumentException>(() => _gateway.CreatePayment(request), "Order memo is required");
        }
    }
}
