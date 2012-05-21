using System.Configuration;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Raven.Abstractions.Indexing;
using Raven.Client;

namespace GroupGiving.Web.App_Start
{
    public static class RavenDbIndexes
    {
        public static void Initialise(IDocumentStore documentStore)
        {
            // only recreate indexes if appSettings say we can
            if (ConfigurationManager.AppSettings["buildRavenIndexesOnStartup"] == null
                || !bool.Parse(ConfigurationManager.AppSettings["buildRavenIndexesOnStartup"]))
            {
                return;
            }

            if (documentStore.DatabaseCommands.GetIndex("contentSearch") != null)
            {
                documentStore.DatabaseCommands.DeleteIndex("contentSearch");
            }
            documentStore.DatabaseCommands.PutIndex("contentSearch",
                                                    new IndexDefinition()
                                                        {
                                                            Map =
                                                                @"from c in docs.PageContents 
from contentDefinition in Hierarchy(c, ""Content"")
from contentByCulture in Hierarchy(contentDefinition, ""ContentByCulture"")
select new {c.Id, c.Address, contentDefinition.Label, contentByCulture.Key, contentByCulture.Value}",
                                                            Analyzers =
                                                                {
                                                                    {"Label", typeof (StopAnalyzer).FullName},
                                                                    {"Value", typeof (StopAnalyzer).FullName},
                                                                }
                                                        });

            if (documentStore.DatabaseCommands.GetIndex("eventSearch") != null)
            {
                documentStore.DatabaseCommands.DeleteIndex("eventSearch");
            }
            documentStore.DatabaseCommands.PutIndex("eventSearch",
                                                    new IndexDefinition()
                                                        {
                                                            Map =
                                                                @"from e in docs.GroupGivingEvents 
select new {e.Id, e.Title, e.State, Location=e.City +"", "" + e.Country, e.City, e.Country, e.SalesEndDateTime, e.StartDate, PledgeCount=e.Pledges.Count}",
                                                            Analyzers =
                                                                {
                                                                    {"Title", typeof (StopAnalyzer).FullName},
                                                                    {"State", typeof (StopAnalyzer).FullName},
                                                                    {"Location", typeof (StopAnalyzer).FullName},
                                                                    {"City", typeof (StopAnalyzer).FullName},
                                                                    {"Country", typeof (StopAnalyzer).FullName},
                                                                    {"StartDate", typeof (WhitespaceAnalyzer).FullName},
                                                                    {"SalesEndDateTime", typeof (WhitespaceAnalyzer).FullName},
                                                                    {"PledgeCount", typeof (StandardAnalyzer).FullName}
                                                                }
                                                        });

            if (documentStore.DatabaseCommands.GetIndex("transactionHistory") != null)
            {
                documentStore.DatabaseCommands.DeleteIndex("transactionHistory");
            }
            documentStore.DatabaseCommands.PutIndex("transactionHistory",
                                                    new IndexDefinition()
                                                        {
                                                            Map =
                                                                @"from e in docs.GroupGivingEvents
from pledge in Hierarchy(e, ""Pledges"")
from history in Hierarchy(pledge, ""PaymentGatewayHistory"")
select new {e.Id, pledge.OrderNumber, pledge.TransactionId, pledge.PaymentStatus, pledge.AccountEmailAddress, history.TimeStamp, history.Request, history.Response}"

                                                        });

            documentStore.DatabaseCommands.PutIndex("pledges", new DenormalizedPledges(), true);
        }

}
}