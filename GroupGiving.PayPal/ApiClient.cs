using System.Linq;
using System.Xml;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
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
        private readonly ISiteConfiguration _siteConfiguration;

        public string AdaptiveAccountsUrl { get { return _siteConfiguration.AdaptiveAccountsConfiguration.ApiBaseUrl + "/AdaptiveAccounts"; } }
        public string AdaptivePaymentsUrl { get { return _siteConfiguration.AdaptiveAccountsConfiguration.ApiBaseUrl + "/AdaptivePayments"; } }

        public ApiClient(ApiClientSettings clientSettings, ISiteConfiguration siteConfiguration)
        {
            _clientSettings = clientSettings;
            _siteConfiguration = siteConfiguration;
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