using System;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class PayPalPaymentGateway : IPaymentGateway
    {
        private readonly IApiClient _apiClient;
        private readonly IPayPalConfiguration _payPalConfiguration;

        public PayPalPaymentGateway(IApiClient apiClient, IPayPalConfiguration payPalConfiguration)
        {
            _apiClient = apiClient;
            _payPalConfiguration = payPalConfiguration;
        }

        public PaymentGatewayResponse MakeRequest(PaymentGatewayRequest request)
        {
            Uri successCallback, failureCallback;
            
            if (!Uri.TryCreate(_payPalConfiguration.SuccessCallbackUrl, UriKind.Absolute, out successCallback))
                throw new ArgumentException("Success callback url is required");

            if (!Uri.TryCreate(_payPalConfiguration.FailureCallbackUrl, UriKind.Absolute, out failureCallback))
                throw new ArgumentException("Failure callback url is required");

            if (string.IsNullOrWhiteSpace(request.OrderMemo))
                throw new ArgumentException("Order memo is required");

            

            var paypalRequest = new PayRequest();
            paypalRequest.ActionType = "PAY";
            paypalRequest.CurrencyCode = "GBP";
            paypalRequest.FeesPayer = "EACHRECEIVER";
            paypalRequest.Memo = request.OrderMemo;
            paypalRequest.CancelUrl = request.FailureCallbackUrl;
            paypalRequest.ReturnUrl = request.SuccessCallbackUrl;
            paypalRequest.Receivers.Add(new Receiver(request.Amount.ToString("#.00"), "seller_1304843436_biz@gmail.com"));
            paypalRequest.Receivers.Add(new Receiver(request.Amount.ToString("#.00"), "sellr2_1304843519_biz@gmail.com"));
            return _apiClient.SendPayRequest(paypalRequest);
        }
    }
}