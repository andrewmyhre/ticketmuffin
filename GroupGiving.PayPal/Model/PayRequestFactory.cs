using GroupGiving.Core.Configuration;
using GroupGiving.Core.PayPal;
using GroupGiving.PayPal.Configuration;

namespace GroupGiving.PayPal.Model
{
    public class PayRequestFactory : IPayRequestFactory
    {
        private readonly AdaptiveAccountsConfiguration _paypalConfiguration;

        public PayRequestFactory(AdaptiveAccountsConfiguration paypalConfiguration)
        {
            _paypalConfiguration = paypalConfiguration;
        }

        public PayRequest RegularPayment(string currencyCode, Receiver[] receivers, string memo)
        {
            return new PayRequest(_paypalConfiguration)
                       {
                           CurrencyCode = currencyCode,
                           Receivers = receivers,
                           Memo=memo,
                           ActionType = "CREATE"
                       };
        }

        public PayRequest ChainedPayment(string currencyCode, Receiver[] receivers, string memo)
        {
            return new PayRequest(_paypalConfiguration)
                       {
                           ActionType = "PAY_PRIMARY",
                           FeesPayer = "EACHRECEIVER",
                           CurrencyCode = currencyCode,
                           Memo=memo,
                           Receivers=receivers
                       };
        }
    }
}