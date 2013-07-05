using System;
using System.Linq;
using TicketMuffin.Core.Payments;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.PayPal.Model;
using Receiver = TicketMuffin.Core.Payments.Receiver;

namespace TicketMuffin.PayPal
{
    public class PayPalPaymentGateway : IPaymentGateway
    {
        private readonly IPayPalApiClient _payPalApiClient;
        private readonly AdaptiveAccountsConfiguration _paypalConiguration;

        public PayPalPaymentGateway(IPayPalApiClient payPalApiClient, 
            AdaptiveAccountsConfiguration paypalConiguration)
        {
            _payPalApiClient = payPalApiClient;
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
                                                         select new TicketMuffin.PayPal.Model.Receiver(r.AmountToReceive.ToString("#.00"), 
                                                             r.EmailAddress,
                                                             r.Primary))
                                                .ToArray()
                                        };

            var response = _payPalApiClient.Payments.SendPayRequest(payRequest);

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

        public object CreateDelayedPayment(object request)
        {
            throw new NotImplementedException();
        }

        TicketMuffin.Core.Payments.PaymentDetailsResponse IPaymentGateway.RetrievePaymentDetails(string transactionId)
        {
            throw new NotImplementedException();
        }

        public IPaymentRefundResponse Refund(string transactionId, decimal amount, string receiverId)
        {
            throw new NotImplementedException();
        }

        public IPaymentCaptureResponse CapturePayment(string transactionId)
        {
            throw new NotImplementedException();
        }

        public PaymentAuthoriseResponse AuthoriseCharge(decimal amount, string currencyCode, string paymentMemo, string recipientId,
                                                         bool capture = false)
        {
            throw new NotImplementedException();
        }

        public PaymentCreationResponse CreatePayment(string memo, string iso4217Alpha3Code, string successUrl, string failureUrl,
                                                     Receiver[] receivers)
        {
            throw new NotImplementedException();
        }

        public string Name { get { return "PayPal"; } }

        public TicketMuffin.Core.Payments.PaymentDetailsResponse RetrievePaymentDetails(string transactionId)
        {
            var paypalPayment = _payPalApiClient.Payments.SendPaymentDetailsRequest(
                new PaymentDetailsRequest(transactionId));

            var response = new TicketMuffin.Core.Payments.PaymentDetailsResponse();

            switch (paypalPayment.status)
            {
                case "INCOMPLETE":
                    response.PaymentStatus = TicketMuffin.Core.Payments.PaymentStatus.AuthorisedUnsettled;
                    break;
            }
            response.SenderId = paypalPayment.senderEmail;
            response.Successful = true;
            return response;
        }

        public RefundResponse Refund(RefundRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PayKey))
            {
                throw new ArgumentException("Transaction Id must be provided", "request.TransactionId");
            }

            RefundResponse response = null;

            return _payPalApiClient.Payments.Refund(new RefundRequest(request.PayKey)
                                                {
                                                    Receivers = new ReceiverList(
                                                        request.Receivers.Select(r=>
                                                            new TicketMuffin.PayPal.Model.Receiver(r.Amount.ToString("#.00"), 
                                                                r.Email, r.Primary)))
                                                 });
        }

        public ExecutePaymentResponse ExecutePayment(ExecutePaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PayKey))
            {
                throw new ArgumentException("transactionid must be provided", "request.transactionid");
            }

            return _payPalApiClient.Payments.SendExecutePaymentRequest(new ExecutePaymentRequest(request.PayKey));
        }
    }
}