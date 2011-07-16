using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _gateway = new PayPalPaymentGateway(null,ValidPayPalConfiguration());
        }

        [Test]
        public void ASuccessCallbackUrlIsRequired()
        {
            PaymentGatewayRequest request=ValidPaymentGatewayRequest();
            request.SuccessCallbackUrl = "";
            
            Assert.Throws<ArgumentException>(()=>_gateway.MakeRequest(request), "Success callback url is required");
        }

        [Test]
        public void ASuccessFailureUrlIsRequired()
        {
            PaymentGatewayRequest request = ValidPaymentGatewayRequest();
            request.FailureCallbackUrl = "";

            Assert.Throws<ArgumentException>(() => _gateway.MakeRequest(request), "Failure callback url is required");
        }

        [Test]
        public void AOrderMemoIsRequired()
        {
            var request = ValidPaymentGatewayRequest();
            request.OrderMemo = "";

            Assert.Throws<ArgumentException>(() => _gateway.MakeRequest(request), "Order memo is required");
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

        protected static IPayPalConfiguration ValidPayPalConfiguration()
        {
            return new PayPalConfiguration()
                       {
                           PayFlowProPaymentPage = "http://somedomain.com/pay",
                           PayPalMerchantPassword = "12345",
                           PayPalMerchantSignature = "12345",
                           PayPalMerchantUsername = "12345"
                       };
        }
    }
}
