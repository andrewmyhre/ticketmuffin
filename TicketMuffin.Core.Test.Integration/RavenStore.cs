using System;
using System.Reflection;
using Raven.Client;
using Raven.Client.Indexes;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Indexes;

namespace TicketMuffin.Core.Test.Integration
{
    public static class RavenStore
    {
        public static IDocumentStore CreateDocumentStore()
        {
            var documentStore = new Raven.Client.Embedded.EmbeddableDocumentStore() {RunInMemory = true};
            documentStore.Initialize();
            new IndexManager().Initialise(documentStore);
            IndexCreation.CreateIndexes(Assembly.GetAssembly(typeof(ContentByCultureAndAddress)), documentStore);

            documentStore.Conventions.RegisterIdConvention<LocalisedContent>((dbname, commands, content) => String.Join("/", "content", content.Culture, content.Address, content.Label));
            documentStore.Conventions.RegisterIdConvention<Currency>((dbname, commands, currency) => "currencies/" + currency.Iso4217NumericCode);

            return documentStore;
        }
    }
}