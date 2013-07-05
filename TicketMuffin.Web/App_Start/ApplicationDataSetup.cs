using System.Web.Hosting;
using Microsoft.Practices.ServiceLocation;
using Raven.Client;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.Services;
using log4net;

namespace TicketMuffin.Web.App_Start
{
    // invoke from the ninject start
    public static class ApplicationDataSetup
    {
        private static ICountryService CountryService;
        private static ISiteConfigurationService SiteConfigurationService;
        private static ILog Logger = LogManager.GetLogger(typeof (ApplicationDataSetup));

        public static void Start()
        {
            using (var session = ServiceLocator.Current.GetInstance<IDocumentSession>())
            {
                CountryService = new CountryService(session);
                SiteConfigurationService = new SiteConfigurationService(session);

                CountryService.EnsureCountryData(HostingEnvironment.MapPath("~/App_Data/countrylist.csv"));
                SiteConfigurationService.EnsureConfigurationDataExists();
            }
        }
    }
}