using System;
using System.Linq;
using GroupGiving.PayPal.Clients;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class PayPalPaymentGateway : IPaymentGateway
    {
        private readonly IApiClient _apiClient;
        private readonly AdaptiveAccountsConfiguration _paypalConiguration;

        public PayPalPaymentGateway(IApiClient apiClient, 
            AdaptiveAccountsConfiguration paypalConiguration)
        {
            _apiClient = apiClient;
            _paypalConiguration = paypalConiguration;
        }

        public PaymentGatewayResponse CreatePayment(PaymentGatewayRequest request)
        {
            return SendPaymentRequest(request, "PAY");
        }

        private PaymentGatewayResponse SendPaymentRequest(PaymentGatewayRequest request, string paymentActionType)
        {
            if (string.IsNullOrWhiteSpace(request.OrderMemo))
            {
                throw new ArgumentException("Order memo must be provided", "request.OrderMemo");
            }

            if (string.IsNullOrWhiteSpace(request.FailureCallbackUrl))
            {
                throw new ArgumentException("Failure callback url must be provided", "request.FailureCallbackUrl");
            }

            if (string.IsNullOrWhiteSpace(request.SuccessCallbackUrl))
            {
                throw new ArgumentException("Success callback url must be provided", "request.SuccessCallbackUrl");
            }

            PayRequest payRequest = new PayRequest()
                                        {
                                            ActionType = paymentActionType,
                                            CancelUrl = request.FailureCallbackUrl,
                                            ReturnUrl = request.SuccessCallbackUrl,
                                            Memo = request.OrderMemo,
                                            CurrencyCode = request.CurrencyCode,
                                            Receivers = (from r in request.Recipients
                                                         orderby r.AmountToReceive descending
                                                         select new Receiver(r.AmountToReceive.ToString("#.00"), 
                                                             r.EmailAddress,
                                                             r.Primary))
                                                .ToArray()
                                        };

            var response = _apiClient.Payments.SendPayRequest(payRequest);

            return new PaymentGatewayResponse()
                       {
                           PaymentExecStatus = response.paymentExecStatus,
                           PaymentPageUrl = string.Format(_paypalConiguration.PayFlowProPaymentPage, response.payKey),
                           payKey = response.payKey,
                           DialogueEntry = ((ResponseBase)response).Raw
                       };
        }

        public PaymentGatewayResponse CreateDelayedPayment(PaymentGatewayRequest request)
        {
            return SendPaymentRequest(request, "PAY_PRIMARY");
        }

        public PaymentDetailsResponse RetrievePaymentDetails(string transactionId)
        {
            var result = _apiClient.Payments.SendPaymentDetailsRequest(
                new PaymentDetailsRequest(transactionId));

            return new PaymentDetailsResponse()
                       {
                           status = result.status,
                           senderEmail= result.senderEmail,
                           Raw = result.Raw
                       };
        }

        public RefundResponse Refund(RefundRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PayKey))
            {
                throw new ArgumentException("Transaction Id must be provided", "request.TransactionId");
            }

            RefundResponse response = null;

            return _apiClient.Payments.Refund(new RefundRequest(request.PayKey)
                                                {
                                                    Receivers = new ReceiverList(
                                                        request.Receivers.Select(r=>
                                                            new Receiver(r.Amount.ToString("#.00"), 
                                                                r.Email, r.Primary)))
                                                 });
        }

        public ExecutePaymentResponse ExecutePayment(ExecutePaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PayKey))
            {
                throw new ArgumentException("transactionid must be provided", "request.transactionid");
            }

            return _apiClient.Payments.SendExecutePaymentRequest(new ExecutePaymentRequest(request.PayKey));
        }
    }
}