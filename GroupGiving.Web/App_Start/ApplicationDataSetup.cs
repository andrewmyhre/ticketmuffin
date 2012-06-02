using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using GroupGiving.Core.Services;
using Raven.Client;

// invoked from application start in global.asax
namespace GroupGiving.Web.App_Start
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