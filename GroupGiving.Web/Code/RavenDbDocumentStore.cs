using System;
using System.Configuration;
using GroupGiving.Core.Services;
using GroupGiving.Web.App_Start;
using Raven.Client;
using Raven.Client.Document;

namespace GroupGiving.Web.Code
{
    public class RavenDbDocumentStore
    {
        private static IDocumentStore _instance;
        public static IDocumentStore Instance
        {
            get
            {
                if (_instance == null)
                {
                        _instance = new DocumentStore() {Url = ConfigurationManager.AppSettings["RavenDbStoragePath"]};
                        _instance.Initialize();
                        RavenDbIndexes.Initialise(_instance);
                        RavenDbAppData.Start(_instance, 
                            new CountryService(_instance),
                            new SiteConfigurationService(_instance));
                }
                return _instance;
            }
        }
    }
}