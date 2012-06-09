using TicketMuffin.PayPal.Model;

namespace TicketMuffin.PayPal.Clients
{
    public class PaymentsClient : IPaymentsApiClient
    {
        private readonly ApiClientSettings _clientSettings;
        public string AdaptivePaymentsUrl { get { return _clientSettings.Configuration.ApiBaseUrl + "/AdaptivePayments"; } }
        public PaymentsClient(ApiClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }
        public PayResponse SendPayRequest(PayRequest request)
        {
            return new HttpChannel().ExecuteRequest<PayRequest, PayResponse>("AdaptivePayments", "Pay", request, _clientSettings);
        }

        public PaymentDetailsResponse SendPaymentDetailsRequest(PaymentDetailsRequest request)
        {
            return new HttpChannel().ExecuteRequest<PaymentDetailsRequest, PaymentDetailsResponse>("AdaptivePayments", "PaymentDetails", request, _clientSettings);
        }

        public ExecutePaymentResponse SendExecutePaymentRequest(ExecutePaymentRequest request)
        {
            return new HttpChannel().ExecuteRequest<ExecutePaymentRequest, ExecutePaymentResponse>("AdaptivePayments", "ExecutePayment", request, _clientSettings);
        }

        public RefundResponse Refund(RefundRequest request)
        {
            return new HttpChannel().ExecuteRequest<RefundRequest, RefundResponse>("AdaptivePayments", "Refund", request, _clientSettings);
        }
    }
}