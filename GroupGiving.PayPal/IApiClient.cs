using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IApiClient
    {
        PayResponse SendPayRequest(PayRequest request);
        PaymentDetailsResponse SendPaymentDetailsRequest(PaymentDetailsRequest request);
        RefundResponse Refund(RefundRequest request);
    }
}