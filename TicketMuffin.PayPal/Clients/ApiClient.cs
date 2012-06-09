using TicketMuffin.PayPal.Configuration;
using log4net;

namespace TicketMuffin.PayPal.Clients
{
    public class ApiClient : IApiClient
    {
        private ILog _log = LogManager.GetLogger(typeof(ApiClient));
        private readonly ApiClientSettings _clientSettings;
        public IAccountsApiClient Accounts { get; set; }
        public IPaymentsApiClient Payments { get; set; }

        public AdaptiveAccountsConfiguration Configuration
        {
            get { return _clientSettings.Configuration; }
        }

        public ApiClient(ApiClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
            Accounts = new AccountsClient(clientSettings);
            Payments = new PaymentsClient(clientSettings);
        }
    }
}