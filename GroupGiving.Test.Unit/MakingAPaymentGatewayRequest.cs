using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
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

    public class PaymentGatewayTestsBase
    {
        protected static PaymentGatewayRequest ValidPaymentGatewayRequest()
        {
            return new PaymentGatewayRequest()
                       {
                           Amount=1,
                           FailureCallbackUrl = "http://somedomain.com/failure",
                           SuccessCallbackUrl = "http://somedomain.com/success",
                           OrderMemo = "Logs of wood"
                       };
        }

        protected static ISiteConfiguration ValidPayPalConfiguration()
        {
            return new SiteConfiguration()
                       {
                           AdaptiveAccountsConfiguration= new AdaptiveAccountsConfiguration()
                            {
                                LivePayFlowProPaymentPage = "http://somedomain.com/pay",
                                SandboxMode = false,
                                ApiUsername = "12345",
                                ApiPassword = "12345",
                                ApiSignature = "12345"
                            }
                       };
        }
    }
}
