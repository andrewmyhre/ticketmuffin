using System;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Raven.Client;
using Raven.Client.Indexes;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Indexes;

namespace TicketMuffin.Core.Conventions
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

            return documentStore;
        }
    }
}