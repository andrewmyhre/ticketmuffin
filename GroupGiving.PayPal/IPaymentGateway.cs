using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IPaymentGateway
    {
        PaymentGatewayResponse MakeRequest(PaymentGatewayRequest request);
    }
}