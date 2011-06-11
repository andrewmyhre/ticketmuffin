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
                    _instance = new DocumentStore() {Url = "http://localhost:8080"};
                    _instance.Initialize();
                }
                return _instance;
            }
        }
    }
}