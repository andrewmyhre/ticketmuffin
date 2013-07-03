using System.Web.Hosting;
using Microsoft.Practices.ServiceLocation;
using Raven.Client;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.Services;

namespace TicketMuffin.Web.App_Start
{
    // invoke from the ninject start
    public static class ApplicationDataSetup
    {
        private static ICountryService CountryService;
        private static ISiteConfigurationService SiteConfigurationService;

        public static void Start()
        {
            var store = ServiceLocator.Current.GetInstance<IDocumentStore>();

            using (var session = store.OpenSession())
            {
                CountryService = new CountryService(session);
                SiteConfigurationService = new SiteConfigurationService(session);

                CountryService.EnsureCountryData(HostingEnvironment.MapPath("~/App_Data/countrylist.csv"));
                SiteConfigurationService.EnsureConfigurationData();
            }
        }
    }
}