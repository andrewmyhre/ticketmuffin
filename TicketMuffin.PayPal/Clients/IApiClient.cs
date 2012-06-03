using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal.Clients
{
    public interface IApiClient
    {
        IPaymentsApiClient Payments { get; set; }
        IAccountsApiClient Accounts { get; set; }
        AdaptiveAccountsConfiguration Configuration { get; }
    }

    public interface IPaymentsApiClient
    {
        PayResponse SendPayRequest(PayRequest request);
        PaymentDetailsResponse SendPaymentDetailsRequest(PaymentDetailsRequest request);
        RefundResponse Refund(RefundRequest request);
        ExecutePaymentResponse SendExecutePaymentRequest(ExecutePaymentRequest request);
        
    }

    public interface IAccountsApiClient
    {
        GetVerifiedStatusResponse VerifyAccount(GetVerifiedStatusRequest request);
        RequestPermissionsResponse RequestPermissions(RequestPermissionsRequest request);
        GetAccessTokenResponse GetAccessToken(GetAccessTokenRequest request);
    }
}