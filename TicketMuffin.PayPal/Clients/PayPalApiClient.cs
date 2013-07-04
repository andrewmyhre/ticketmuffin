using TicketMuffin.PayPal.Configuration;
using log4net;

namespace TicketMuffin.PayPal.Clients
{
    public class PayPalApiClient : IPayPalApiClient
    {
        private ILog _log = LogManager.GetLogger(typeof(PayPalApiClient));
        private readonly ApiClientSettings _clientSettings;
        public IAccountsApiClient Accounts { get; set; }
        public IPaymentsApiClient Payments { get; set; }

        public AdaptiveAccountsConfiguration Configuration
        {
            get { return _clientSettings.Configuration; }
        }

        public PayPalApiClient(ApiClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
            Accounts = new AccountsClient(clientSettings);
            Payments = new PaymentsClient(clientSettings);
        }
    }
}