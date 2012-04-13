using GroupGiving.Core.PayPal;

namespace GroupGiving.PayPal.Model
{
    public interface IPayRequestFactory
    {
        PayRequest RegularPayment(string currencyCode, Receiver[] receivers, string memo);
        PayRequest ChainedPayment(string currencyCode, Receiver[] receivers, string memo);
    }
}