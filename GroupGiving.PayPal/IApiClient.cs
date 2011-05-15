using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public interface IApiClient
    {
        PayResponse SendPayRequest(PayRequest request);
    }
}