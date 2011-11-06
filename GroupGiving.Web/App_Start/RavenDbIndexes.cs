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
            if (documentStore.DatabaseCommands.GetIndex("contentSearch") == null)
            {
                documentStore.DatabaseCommands.DeleteIndex("contentSearch");

                documentStore.DatabaseCommands.PutIndex("contentSearch",
                    new IndexDefinition()
                    {
                        Map = @"from c in docs.PageContents 
from contentDefinition in Hierarchy(c, ""Content"")
from contentByCulture in Hierarchy(contentDefinition, ""ContentByCulture"")
select new {c.Id, c.Address, contentDefinition.Label, contentByCulture.Key, contentByCulture.Value}",
                        Analyzers =
                                {
                                    {"Label", typeof(StopAnalyzer).FullName},
                                    {"Value", typeof(StopAnalyzer).FullName},
                                }
                    });
            }
        }
    }
}