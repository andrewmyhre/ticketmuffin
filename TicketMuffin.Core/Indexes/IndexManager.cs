using System.Configuration;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using System.Linq;
using Raven.Database.Linq.PrivateExtensions;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Indexes
{
    public class IndexManager
    {
        public void Initialise(IDocumentStore documentStore)
        {
            // only recreate indexes if appSettings say we can
            if (ConfigurationManager.AppSettings["buildRavenIndexesOnStartup"] == null
                || !bool.Parse(ConfigurationManager.AppSettings["buildRavenIndexesOnStartup"]))
            {
                return;
            }

            documentStore.DatabaseCommands.PutIndex("contentSearch",
                                                    new IndexDefinitionBuilder<LocalisedContent>
                                                        {
                                                            Map = pages => from p in pages 
                                                                           select new {p.Id, p.Culture, p.Address, p.Value},
                                                                           Analyzers = {
                                                                                { x=>x.Value, "SimpleAnalyzer"}
                                                                            }
                                                        }, true);

            documentStore.DatabaseCommands.PutIndex("eventSearch",
                                                    new IndexDefinitionBuilder<GroupGivingEvent>()
                                                    {
                                                        Map = events => from e in events
                                                                        select new {e.Id, e.Title, e.State, Location=e.City +", " + e.Country, e.City, e.Country, e.SalesEndDateTime, e.StartDate, PledgeCount=e.Pledges.Count}
                                                    }, true);

            documentStore.DatabaseCommands.PutIndex("pledges", new DenormalizedPledges(), true);
        }
    }
}
