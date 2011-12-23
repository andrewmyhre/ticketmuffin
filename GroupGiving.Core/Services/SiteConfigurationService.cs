using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Core.Services
{
    public class SiteConfigurationService : ISiteConfigurationService
    {
        private readonly IDocumentStore _documentStore;

        public SiteConfigurationService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public ISiteConfiguration GetConfiguration()
        {
            using (var session = _documentStore.OpenSession())
            {
                var configuration = session.Query<SiteConfiguration>().FirstOrDefault();
                if (configuration == null)
                {
                    configuration = new SiteConfiguration();
                    configuration.PayFlowProConfiguration = new PayFlowProConfiguration();
                    configuration.AdaptiveAccountsConfiguration = new AdaptiveAccountsConfiguration();
                    configuration.JustGivingApiConfiguration = new JustGivingApiConfiguration();
                    configuration.DatabaseConfiguration = new DatabaseConfiguration();
                    session.Store(configuration);
                    session.SaveChanges();

                }
                return configuration;
            }
        }
    }
}
