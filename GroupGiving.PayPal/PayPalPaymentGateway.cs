using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using ExecutePaymentRequest = GroupGiving.Core.Actions.ExecutePayment.ExecutePaymentRequest;
using ExecutePaymentResponse = GroupGiving.Core.Actions.ExecutePayment.ExecutePaymentResponse;
using PaymentDetailsResponse = GroupGiving.Core.Dto.PaymentDetailsResponse;
using RefundRequest = GroupGiving.Core.Dto.RefundRequest;
using RefundResponse = GroupGiving.Core.Dto.RefundResponse;

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

            var response = _apiClient.SendPayRequest(payRequest);

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

        public PaymentDetailsResponse RetrievePaymentDetails(Core.Dto.PaymentDetailsRequest request)
        {
            var result = _apiClient.SendPaymentDetailsRequest(
                new PaymentDetailsRequest(request.TransactionId));

            return new PaymentDetailsResponse()
                       {
                           Status = result.status,
                           SenderEmailAddress = result.senderEmail,
                           RawResponse = result,
                           DialogueEntry = ((ResponseBase)result).Raw
                       };
        }

        public RefundResponse Refund(RefundRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionId))
            {
                throw new ArgumentException("Transaction Id must be provided", "request.TransactionId");
            }

            Model.RefundResponse response = null;

            try
            {
                response = _apiClient.Refund(new Model.RefundRequest(request.TransactionId)
                                                 {
                                                     Receivers = new ReceiverList(request.Receivers.Select(r=>new Receiver(r.AmountToReceive.ToString("#.00"), r.EmailAddress, r.Primary)))
                                                 });
            } catch (HttpChannelException ex)
            {
                return new RefundResponse()
                           {
                               Successful = false,
                               RawResponse = ex,
                               DialogueEntry = ((ResponseBase)ex.FaultMessage).Raw
                           };
            } catch (Exception ex)
            {
                return new RefundResponse()
                {
                    Successful = false,
                    RawResponse = ex
                };
            }

            return new RefundResponse()
                       {
                           Successful = response.ResponseEnvelope.ack.StartsWith("Success")
                            && response.refundInfoList.All(ri => ri.refundStatus == "REFUNDED" 
                                || ri.refundStatus == "REFUNDED_PENDING" 
                                || ri.refundStatus == "NOT_PAID" 
                                || ri.refundStatus == "ALREADY_REVERSED_OR_REFUNDED"),
                           RawResponse = response,
                           DialogueEntry = ((ResponseBase)response).Raw,
                           Message = response.refundInfoList.First().refundStatus
                       };
        }

        public ExecutePaymentResponse ExecutePayment(ExecutePaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionId))
            {
                throw new ArgumentException("transactionid must be provided", "request.transactionid");
            }

            Model.ExecutePaymentResponse response = null;
            

            try
            {
                response = _apiClient.SendExecutePaymentRequest(new Model.ExecutePaymentRequest(request.TransactionId));
            } catch (HttpChannelException exception)
            {
                return new ExecutePaymentResponse()
                           {
                               Successful = false,
                               RawResponse = exception,
                               DialogueEntry = ((ResponseBase) exception.FaultMessage).Raw
                           };
            } catch (Exception exception)
            {
                return new ExecutePaymentResponse()
                           {
                               Successful = false,
                               RawResponse = exception
                           };
            }

            return new ExecutePaymentResponse()
                       {
                           Successful = true,
                           DialogueEntry = ((ResponseBase) response).Raw
                       };
        }
    }
}