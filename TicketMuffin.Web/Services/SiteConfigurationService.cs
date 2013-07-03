using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.Web.Configuration;
using log4net;

namespace TicketMuffin.Web.Services
{
    public class SiteConfigurationService : ISiteConfigurationService
    {
        private readonly IDocumentSession _session;
        private ILog _log = LogManager.GetLogger(typeof (SiteConfigurationService));

        public SiteConfigurationService(IDocumentSession session)
        {
            _session = session;
        }

        public ISiteConfiguration GetConfiguration()
        {
            _log.Debug("loading site configuration");
            ISiteConfiguration configuration = _session.Query<SiteConfiguration>().FirstOrDefault();
            configuration = EnsureValidConfigurationObject(configuration, _session);
            return configuration;
        }

        public void EnsureConfigurationData()
        {
            var configuration = _session.Query<SiteConfiguration>().FirstOrDefault();
            if (configuration == null)
            {
                _log.Info("No site configuration found, creating");
                configuration = CreateDefaultConfiguration();
                _session.Store(configuration);
                _session.SaveChanges();
                _log.Info("Created default configuration");
            }
        }

        private SiteConfiguration CreateDefaultConfiguration()
        {
            return new SiteConfiguration()
            {
                EventImagePathFormat = "~/eventImages/{0}/eventImage.jpg",
                LoginUrl = "~/signin",
                AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration()
                {
                    ApiPassword = "1321277160",
                    ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                    ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                    SandboxApplicationId = "APP-80W284485P519543T",
                    LiveApplicationId = "APP-8YG236387E473230W",
                    DeviceIpAddress = "127.0.0.1",
                    SandboxApiBaseUrl = "https://svcs.sandbox.paypal.com/",
                    LiveApiBaseUrl = "https://svcs.paypal.com/",
                    RequestDataFormat = "SOAP11",
                    ResponseDataFormat = "SOAP11",
                    SandboxMailAddress = "nothing",
                    SandboxMode = true,
                    SuccessCallbackUrl = "/Order/Success?payKey=${payKey}",
                    FailureCallbackUrl = "/Order/Cancel?payKey=${payKey}",
                    SandboxPayFlowProPaymentPage = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&paykey={0}",
                    LivePayFlowProPaymentPage = "https://www.paypal.com/webscr?cmd=_ap-payment&paykey={0}",
                    ApiVersion = "1.1.0",
                    RequestDataBinding = "XML",
                    ResponseDataBinding = "XML"

                },
                DatabaseConfiguration = new DatabaseConfiguration()
                {
                    StorageLocation = "http://localhost:8080"
                },
                JustGivingApiConfiguration = new JustGivingApiConfiguration()
                {
                    JustGivingApiKey = "d0032bfe",
                    JustGivingApiDomainBase = "https://api.staging.justgiving.com/",
                    JustGivingApiVersion = "1"
                },
            };
        }

        private ISiteConfiguration EnsureValidConfigurationObject(ISiteConfiguration configuration, IDocumentSession session)
        {
            bool storeNew = false, update=false;
            if (configuration == null)
            {
                configuration = CreateDefaultConfiguration();
                storeNew = true;
            }
            if (configuration.AdaptiveAccountsConfiguration == null)
            {
                configuration.AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration();
                update = true;
            }
            if (configuration.JustGivingApiConfiguration == null)
            {
                configuration.JustGivingApiConfiguration = new JustGivingApiConfiguration();
                update = true;
            }
            if (configuration.DatabaseConfiguration == null)
            {
                configuration.DatabaseConfiguration = new DatabaseConfiguration();
                update = true;
            }

            if (storeNew)
            {
                session.Store(configuration);
                session.SaveChanges();
            }

            if (update)
            {
                session.SaveChanges();
            }

            return configuration;
        }
    }
}
