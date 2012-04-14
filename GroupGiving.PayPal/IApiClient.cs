using GroupGiving.Core.PayPal;

namespace GroupGiving.PayPal
{
    public interface IApiClient
    {
        PayResponse SendPayRequest(PayRequest request);
        PaymentDetailsResponse SendPaymentDetailsRequest(PaymentDetailsRequest request);
        RefundResponse Refund(RefundRequest request);
        ExecutePaymentResponse SendExecutePaymentRequest(ExecutePaymentRequest request);
        GetVerifiedStatusResponse VerifyAccount(GetVerifiedStatusRequest request);
        RequestPermissionsResponse RequestPermissions(RequestPermissionsRequest request);
        GetAccessTokenResponse GetAccessToken(GetAccessTokenRequest request);
    }
}