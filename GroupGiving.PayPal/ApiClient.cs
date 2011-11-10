using System.Linq;
using System.Xml;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal.Model;
using log4net;
using PaymentDetailsResponse = GroupGiving.PayPal.Model.PaymentDetailsResponse;
using RefundRequest = GroupGiving.PayPal.Model.RefundRequest;
using RefundResponse = GroupGiving.PayPal.Model.RefundResponse;

namespace GroupGiving.PayPal
{
    public class ApiClient : IApiClient
    {
        private ILog _log = LogManager.GetLogger(typeof (ApiClient));
        private readonly ApiClientSettings _clientSettings;
        private readonly IPayPalConfiguration _payPalConfiguration;

        public ApiClient(ApiClientSettings clientSettings, IPayPalConfiguration payPalConfiguration)
        {
            _clientSettings = clientSettings;
            _payPalConfiguration = payPalConfiguration;
        }

        public PayResponse SendPayRequest(PayRequest request)
        {
            // set first receiver as primary
            request.Receivers.First().Primary = true;

            return new HttpChannel().ExecuteRequest<PayRequest, PayResponse>("Pay", request, _clientSettings);
        }

        public PaymentDetailsResponse SendPaymentDetailsRequest(PaymentDetailsRequest request)
        {
            return new HttpChannel().ExecuteRequest<PaymentDetailsRequest, PaymentDetailsResponse>("PaymentDetails", request, _clientSettings);
        }

        public ExecutePaymentResponse SendExecutePaymentRequest(ExecutePaymentRequest request)
        {
            return new HttpChannel().ExecuteRequest<ExecutePaymentRequest, ExecutePaymentResponse>("ExecutePayment", request, _clientSettings);
        }

        public RefundResponse Refund(RefundRequest request)
        {
            return new HttpChannel().ExecuteRequest<RefundRequest, RefundResponse>("Refund", request, _clientSettings);
        }
    }
}