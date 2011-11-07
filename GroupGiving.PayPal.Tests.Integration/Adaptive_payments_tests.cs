using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal.Model;
using NUnit.Framework;
using RefundRequest = GroupGiving.PayPal.Model.RefundRequest;

namespace GroupGiving.PayPal.Tests.Integration
{
    [TestFixture]
    public class Adaptive_payments_tests
    {
        private ApiClientSettings _apiSettings;
        private PayPalConfiguration _paypalConfiguration;
        private ApiClient _apiClient;
        private PayPalPaymentGateway _gateway;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            log4net.Config.XmlConfigurator.Configure();

            _apiSettings = new ApiClientSettings("seller_1304843436_biz_api1.gmail.com", "1304843443",
                                                 "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7");

            _paypalConfiguration = ConfigurationManager.GetSection("paypal") as PayPalConfiguration;
            _apiClient = new ApiClient(_apiSettings, _paypalConfiguration);
            _gateway = new PayPalPaymentGateway(_apiClient, _paypalConfiguration);
        }

        [Test]
        public void Can_create_a_chained_payment()
        {
            var response = _apiClient.SendPayRequest(
                new PayRequest()
                    {
                        ActionType="PAY",
                        CancelUrl = _paypalConfiguration.FailureCallbackUrl,
                        ReturnUrl = _paypalConfiguration.SuccessCallbackUrl,
                        Memo = "test payment " + Guid.NewGuid().ToString(),
                        CurrencyCode = "GBP",
                        Receivers = new []
                                        {
                                            new Receiver("100", "seller_1304843436_biz@gmail.com", true),
                                            new Receiver("90", "sellr2_1304843519_biz@gmail.com", false)
                                        }
                    });

            Assert.That(response.paymentExecStatus, Is.StringMatching("CREATED"));
            System.Diagnostics.Debug.Write(response.payKey, "transaction id");
        }

        [Test]
        public void Can_check_the_details_of_a_payment()
        {
            string memoField = "test payment " + Guid.NewGuid().ToString();
            var response = _apiClient.SendPayRequest(
                new PayRequest()
                    {
                        ActionType = "PAY",
                        CancelUrl = _paypalConfiguration.FailureCallbackUrl,
                        ReturnUrl = _paypalConfiguration.SuccessCallbackUrl,
                        Memo = memoField,
                        CurrencyCode = "GBP",
                        Receivers = new []
                                        {
                                            new Receiver("100", "seller_1304843436_biz@gmail.com", true),
                                            new Receiver("90", "sellr2_1304843519_biz@gmail.com", false)
                                        }
                    });

            var detailsResponse = _apiClient.SendPaymentDetailsRequest(new PaymentDetailsRequest(response.payKey));


            Assert.That(detailsResponse, Is.Not.Null);
            Assert.That(detailsResponse.memo, Is.StringMatching(memoField));

        }

        [TestCase("AP-8YM45525JA853393V")]
        [TestCase("AP-38W574546F953893A")]
        public void Can_check_the_details_of_an_approved_payment(string payKey)
        {
            var detailsResponse = _apiClient.SendPaymentDetailsRequest(new PaymentDetailsRequest(payKey));


            Assert.That(detailsResponse, Is.Not.Null);
        }

        [Test]
        public void Can_refund_a_payment()
        {
            string memoField = "test payment " + Guid.NewGuid().ToString();
            var response = _apiClient.SendPayRequest(
                new PayRequest()
                {
                    ActionType = "PAY",
                    CancelUrl = _paypalConfiguration.FailureCallbackUrl,
                    ReturnUrl = _paypalConfiguration.SuccessCallbackUrl,
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
                new PayRequest()
                    {
                        ActionType = "PAY_PRIMARY",
                        CancelUrl = _paypalConfiguration.FailureCallbackUrl,
                        CurrencyCode = "GBP",
                        FeesPayer = "EACHRECEIVER",
                        Memo = "test payment " + Guid.NewGuid().ToString(),
                        ReturnUrl = _paypalConfiguration.SuccessCallbackUrl,
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
                new PayRequest()
                {
                    ActionType = "PAY_PRIMARY",
                    CancelUrl = _paypalConfiguration.FailureCallbackUrl,
                    CurrencyCode = "GBP",
                    FeesPayer = "EACHRECEIVER",
                    Memo = "test payment " + Guid.NewGuid().ToString(),
                    ReturnUrl = _paypalConfiguration.SuccessCallbackUrl,
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
