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
    public class RavenDbAppData
    {
        public static void Start(IDocumentStore documentStore, ICountryService countryService)
        {
            countryService.EnsureCountryData(HostingEnvironment.MapPath("~/App_Data/countrylist.csv"));
        }
    }
}