using GroupGiving.Core.Actions.ExecutePayment;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;

namespace GroupGiving.Core.Services
{
    public interface IPaymentGateway
    {
        PaymentGatewayResponse CreatePayment(PaymentGatewayRequest request);
        PaymentGatewayResponse CreateDelayedPayment(PaymentGatewayRequest request);
        PaymentDetailsResponse RetrievePaymentDetails(PaymentDetailsRequest request);
        RefundResponse Refund(RefundRequest request);
        ExecutePaymentResponse ExecutePayment(ExecutePaymentRequest request);
    }
}