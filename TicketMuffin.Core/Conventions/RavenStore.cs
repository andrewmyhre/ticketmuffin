using Raven.Client;
using TicketMuffin.Core.Indexes;

namespace TicketMuffin.Core.Conventions
{
    public static class RavenStore
    {
        public static IDocumentStore CreateDocumentStore()
        {
            var documentStore = new Raven.Client.Document.DocumentStore()
                {
                    Url = "http://localhost:8080"
                };
            documentStore.Initialize();
            new IndexManager().Initialise(documentStore);
            return documentStore;
        }
    }
}