using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using NUnit.Framework;
using RefundRequest = GroupGiving.PayPal.Model.RefundRequest;

namespace GroupGiving.PayPal.Tests.Integration
{
    [TestFixture]
    public class Adaptive_payments_tests
    {
        private ApiClientSettings _apiSettings;
        private AdaptiveAccountsConfiguration _paypalConfiguration;
        private ApiClient _apiClient;
        private PayPalPaymentGateway _gateway;
        private IPayRequestFactory _payRequestFactory;
        private Receiver[] _receivers;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            log4net.Config.XmlConfigurator.Configure();

            _paypalConfiguration = ValidSiteConfiguration();
            _apiSettings = new ApiClientSettings(_paypalConfiguration);

            _payRequestFactory = new PayRequestFactory(_paypalConfiguration);

            _apiClient = new ApiClient(_apiSettings, new SiteConfiguration{AdaptiveAccountsConfiguration=_paypalConfiguration});
            _gateway = new PayPalPaymentGateway(_apiClient, _paypalConfiguration);

            _receivers = new[]
                             {
                                 new Receiver("100", "Muffin_1321277131_biz@gmail.com", true),
                                 new Receiver("90", "Raiser_1321277286_biz@gmail.com", false)
                             };
        }

        private AdaptiveAccountsConfiguration ValidSiteConfiguration()
        {
            return new AdaptiveAccountsConfiguration()
                {
                    FailureCallbackUrl = "http://somedomain.com/failure",
                    SuccessCallbackUrl = "http://somedomain.com/success",
                    ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                    ApiPassword = "1321277160",
                    ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                    ApiVersion="1.1.0",
                    SandboxApiBaseUrl = "https://svcs.sandbox.paypal.com/AdaptivePayments",
                    SandboxApplicationId = "APP-80W284485P519543T",
                    SandboxMode = true,
                    SandboxPayFlowProPaymentPage = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}",
                    RequestDataBinding = "XML",
                    ResponseDataBinding = "XML",
                    PayPalAccountEmail = "something@something.com"
                };
        }

        [Test]
        public void Can_create_a_chained_payment()
        {
            var response = _apiClient.SendPayRequest(
                _payRequestFactory.ChainedPayment("GBP", _receivers,
                                    "test payment " + Guid.NewGuid().ToString()
                    ));

            Assert.That(response.paymentExecStatus, Is.StringMatching("CREATED"));
            System.Diagnostics.Debug.Write(response.payKey, "transaction id");
        }

        [Test]
        public void Can_check_the_details_of_a_payment()
        {
            string memoField = "test payment " + Guid.NewGuid().ToString();
            var response = _apiClient.SendPayRequest(
                _payRequestFactory.RegularPayment("GBP", _receivers, memoField));

            var detailsResponse = _apiClient.SendPaymentDetailsRequest(new PaymentDetailsRequest(response.payKey));


            Assert.That(detailsResponse, Is.Not.Null);
            Assert.That(detailsResponse.memo, Is.StringMatching(memoField));

        }

        [Test]
        public void Can_refund_a_payment()
        {
            string memoField = "test payment " + Guid.NewGuid().ToString();
            var response = _apiClient.SendPayRequest(
                new PayRequest(_paypalConfiguration)
                {
                    Memo = memoField,
                    CurrencyCode = "GBP",
                    Receivers = new []
                                        {
                                            new Receiver("100", "seller_1304843436_biz@gmail.com", true),
                                            new Receiver("90", "sellr2_1304843519_biz@gmail.com", false)
                                        }
                });

            System.Diagnostics.Debug.Write(response.payKey, "paykey");
            System.Diagnostics.Debug.WriteLine(string.Format("https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}", response.payKey), "authorization url");

            var refundResponse = _apiClient.Refund(new RefundRequest(response.payKey));

            System.Diagnostics.Debug.WriteLine(refundResponse.ResponseEnvelope.ack, "ack");

            Assert.That(refundResponse, Is.Not.Null);
            Assert.That(refundResponse.ResponseEnvelope.ack, Is.StringMatching("Success"));


        }

        [Test]
        [Ignore("We don't have delayed payments yet")]
        public void Can_create_a_delayed_payment()
        {
            var response = _apiClient.SendPayRequest(
                new PayRequest(_paypalConfiguration)
                    {
                        ActionType = "PAY_PRIMARY",
                        CurrencyCode = "GBP",
                        FeesPayer = "EACHRECEIVER",
                        Memo = "test payment " + Guid.NewGuid().ToString(),
                        Receivers = new []
                                        {
                                            new Receiver("100", "seller_1304843436_biz@gmail.com", true),
                                            new Receiver("95", "sellr2_1304843519_biz@gmail.com", false)
                                        }
                    });

            Assert.That(response.paymentExecStatus, Is.StringMatching("CREATED"));
            
            System.Diagnostics.Debug.Write(response.payKey, "paykey");

            var paymentDetails = _apiClient.SendPaymentDetailsRequest(new PaymentDetailsRequest(response.payKey));

            Assert.That(paymentDetails.status, Is.StringMatching("INCOMPLETE"));
        }

        [Test]
        [Ignore("We don't have delayed payments yet")]
        public void Can_complete_a_delayed_payment()
        {
            var response = _apiClient.SendPayRequest(
                new PayRequest(_paypalConfiguration)
                {
                    ActionType = "PAY_PRIMARY",
                    CurrencyCode = "GBP",
                    FeesPayer = "EACHRECEIVER",
                    Memo = "test payment " + Guid.NewGuid().ToString(),
                    Receivers = new []
                                        {
                                            new Receiver("100", "seller_1304843436_biz@gmail.com", true),
                                            new Receiver("95", "sellr2_1304843519_biz@gmail.com", false)
                                        }
                });

            Assert.That(response.paymentExecStatus, Is.StringMatching("CREATED"));

            System.Diagnostics.Debug.WriteLine(response.payKey, "paykey");
            System.Diagnostics.Debug.WriteLine(string.Format("https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}", response.payKey), "authorization url");

            var paymentDetails = _apiClient.SendPaymentDetailsRequest(new PaymentDetailsRequest(response.payKey));

            var executePaymentResponse = _apiClient.SendExecutePaymentRequest(new ExecutePaymentRequest(response.payKey));
        }
    }
}
