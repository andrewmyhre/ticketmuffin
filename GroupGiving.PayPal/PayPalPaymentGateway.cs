using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
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

        /*
        public IPaymentGatewayResponse MakeRequest(PaymentGatewayRequest request)
        {
            Uri successCallback, failureCallback;
            
            if (!Uri.TryCreate(_payPalConfiguration.SuccessCallbackUrl, UriKind.Absolute, out successCallback))
                throw new ArgumentException("Success callback url is required");

            if (!Uri.TryCreate(_payPalConfiguration.FailureCallbackUrl, UriKind.Absolute, out failureCallback))
                throw new ArgumentException("Failure callback url is required");

            if (string.IsNullOrWhiteSpace(request.OrderMemo))
                throw new ArgumentException("Order memo is required");

            

            var paypalRequest = new PayRequest();
            paypalRequest.ActionType = PayRequestActionType(request.ActionType);
            paypalRequest.CurrencyCode = "GBP";
            paypalRequest.FeesPayer = "EACHRECEIVER";
            paypalRequest.Memo = request.OrderMemo;
            paypalRequest.CancelUrl = request.FailureCallbackUrl;
            paypalRequest.ReturnUrl = request.SuccessCallbackUrl;
            foreach(var recipient in request.Recipients)
            {
                paypalRequest.Receivers.Add(new Receiver(recipient.AmountToReceive.ToString("#.00"), recipient.EmailAddress));
            }
            return _apiClient.SendPayRequest(paypalRequest);
        }*/

        private string PayRequestActionType(PaymentGatewayRequest.ActionTypeEnum type)
        {
            switch (type)
            {
                case PaymentGatewayRequest.ActionTypeEnum.Immediate:
                    return "PAY";
                case PaymentGatewayRequest.ActionTypeEnum.Delayed:
                    return "PAY_PRIMARY";
            }
            return "";
        }

        public PaymentGatewayResponse CreatePayment(PaymentGatewayRequest request)
        {
            PayRequest payRequest = new PayRequest()
            {
                ActionType = "PAY",
                CancelUrl = _payPalConfiguration.FailureCallbackUrl,
                ReturnUrl = _payPalConfiguration.SuccessCallbackUrl,
                Memo = request.OrderMemo,
                CurrencyCode = "GBP",
                Receivers = (from r in request.Recipients orderby r.AmountToReceive descending select new Receiver(r.AmountToReceive.ToString("#.00"), r.EmailAddress)).ToArray()
            };

            var response = _apiClient.SendPayRequest(payRequest);

            return new PaymentGatewayResponse()
                       {
                           PaymentExecStatus = response.paymentExecStatus,
                           PaymentPageUrl = string.Format(_payPalConfiguration.PayFlowProPaymentPage, response.payKey),
                           payKey = response.payKey
                       };
        }

        public TResponse RetrievePaymentDetails<TRequest, TResponse>(TRequest request)
        {
            throw new NotImplementedException();
        }

        public TResponse Refund<TRequest, TResponse>(TRequest request)
        {
            throw new NotImplementedException();
        }
    }
}