using System;
using System.Configuration;
using GroupGiving.Web.App_Start;
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
                    try
                    {
                        _instance = new DocumentStore() {Url = ConfigurationManager.AppSettings["RavenDbStoragePath"]};
                        _instance.Initialize();
                        RavenDbIndexes.Initialise(_instance);
                    } catch
                    {
                        throw new Exception("Couldn't connect to database. Is it running?");
                    }
                }
                return _instance;
            }
        }
    }
}