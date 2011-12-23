using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using Raven.Client;
using log4net;

namespace GroupGiving.Core.Services
{
    public class SiteConfigurationService : ISiteConfigurationService
    {
        private ILog _log = LogManager.GetLogger(typeof (SiteConfigurationService));
        private readonly IDocumentStore _documentStore;

        public SiteConfigurationService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public ISiteConfiguration GetConfiguration()
        {
            using (var session = _documentStore.OpenSession())
            {
                _log.Debug("loading site configuration");
                var configuration = session.Query<SiteConfiguration>().FirstOrDefault();
                EnsureValidConfigurationObject(configuration, session);
                return configuration;
            }
        }

        private void EnsureValidConfigurationObject(ISiteConfiguration configuration, IDocumentSession session)
        {
            bool storeNew = false, update=false;
            if (configuration == null)
            {
                configuration = new SiteConfiguration();
                storeNew = true;
            }
            if (configuration.PayFlowProConfiguration == null)
            {
                configuration.PayFlowProConfiguration = new PayFlowProConfiguration();
                update = true;
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
        }
    }
}
