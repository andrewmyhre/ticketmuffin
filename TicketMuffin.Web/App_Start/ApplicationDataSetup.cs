using System.Web.Hosting;
using TicketMuffin.Core.Services;

// invoked from application start in global.asax
namespace TicketMuffin.Web.App_Start
{

    public class ApplicationDataSetup
    {
        private readonly ICountryService _countryService;
        private readonly ISiteConfigurationService _siteConfigurationService;

        public ApplicationDataSetup(ICountryService countryService,
            ISiteConfigurationService siteConfigurationService)
        {
            _countryService = countryService;
            _siteConfigurationService = siteConfigurationService;
        }

        public void Start()
        {
            _countryService.EnsureCountryData(HostingEnvironment.MapPath("~/App_Data/countrylist.csv"));
            _siteConfigurationService.EnsureConfigurationData();
        }
    }
}