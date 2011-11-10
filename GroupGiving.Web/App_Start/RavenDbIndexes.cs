using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client;

namespace GroupGiving.Web.App_Start
{
    public static class RavenDbIndexes
    {
        public static void Initialise(IDocumentStore documentStore)
        {
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
select new {e.Id, e.Title, e.State, e.City, e.Country, e.StartDate}",
                                                            Analyzers =
                                                                {
                                                                    {"Title", typeof (StopAnalyzer).FullName},
                                                                    {"State", typeof (StopAnalyzer).FullName},
                                                                    {"City", typeof (StopAnalyzer).FullName},
                                                                    {"Country", typeof (StopAnalyzer).FullName},
                                                                    {"StartDate", typeof (WhitespaceAnalyzer).FullName}
                                                                }
                                                        });
        }
    }
}