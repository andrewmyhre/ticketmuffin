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

        public void EnsureConfigurationDataExists()
        {
            var configuration = _session.Query<SiteConfiguration>().SingleOrDefault(c=>c.Mode=="Live");
            if (configuration == null)
            {
                _log.Info("No live configuration found, creating");
                configuration = CreateDefaultConfiguration();
                _session.Store(configuration);
                _session.SaveChanges();
                _log.Info("Created live configuration");
            }

            configuration = _session.Query<SiteConfiguration>().SingleOrDefault(c => c.Mode == "Sandbox");
            if (configuration == null)
            {
                _log.Info("No sandbox configuration found, creating");
                configuration = CreateSandboxConfiguration();
                _session.Store(configuration);
                _session.SaveChanges();
                _log.Info("Created sandbox configuration");
            }
        }

        private SiteConfiguration CreateDefaultConfiguration()
        {
            return new SiteConfiguration()
            {
                Mode = "Live",
                EventImagePathFormat = "~/eventImages/{0}/eventImage.jpg",
                LoginUrl = "~/signin",
                AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration()
                {
                    ApiPassword = "1321277160",
                    ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                    ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                    ApplicationId = "APP-8YG236387E473230W",
                    DeviceIpAddress = "127.0.0.1",
                    ApiBaseUrl = "https://svcs.paypal.com/",
                    RequestDataFormat = "SOAP11",
                    ResponseDataFormat = "SOAP11",
                    SandboxMailAddress = "nothing",
                    SuccessCallbackUrl = "/Order/Success?payKey=${payKey}",
                    FailureCallbackUrl = "/Order/Cancel?payKey=${payKey}",
                    PayFlowProPaymentPage = "https://www.paypal.com/webscr?cmd=_ap-payment&paykey={0}",
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

        private SiteConfiguration CreateSandboxConfiguration()
        {
            return new SiteConfiguration()
            {
                Mode="Sandbox",
                EventImagePathFormat = "~/eventImages/{0}/eventImage.jpg",
                LoginUrl = "~/signin",
                AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration()
                {
                    ApiPassword = "1321277160",
                    ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                    ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                    ApplicationId = "APP-80W284485P519543T",
                    DeviceIpAddress = "127.0.0.1",
                    ApiBaseUrl = "https://svcs.sandbox.paypal.com/",
                    RequestDataFormat = "SOAP11",
                    ResponseDataFormat = "SOAP11",
                    SandboxMailAddress = "nothing",
                    SuccessCallbackUrl = "/Order/Success?payKey=${payKey}",
                    FailureCallbackUrl = "/Order/Cancel?payKey=${payKey}",
                    PayFlowProPaymentPage = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&paykey={0}",
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
