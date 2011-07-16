using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IApiClient
    {
        PaymentGatewayResponse SendPayRequest(PayRequest request);
    }
}