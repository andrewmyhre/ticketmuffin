using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IPaymentGateway
    {
        PaymentGatewayResponse CreatePayment(PaymentGatewayRequest request);
        PaymentGatewayResponse CreateDelayedPayment(PaymentGatewayRequest request);
        PaymentDetailsResponse RetrievePaymentDetails(string transactionId);
        RefundResponse Refund(RefundRequest request);
        ExecutePaymentResponse ExecutePayment(ExecutePaymentRequest request);
    }
}