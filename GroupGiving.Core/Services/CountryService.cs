using System.Collections.Generic;
using System.Linq;
using Raven.Client;

namespace GroupGiving.Core.Services
{
    public class CountryService : ICountryService
    {
        private readonly IDocumentStore _store;

        public CountryService(IDocumentStore store)
        {
            _store = store;
        }

        public IEnumerable<Country> RetrieveAllCountries()
        {
            using (var session = _store.OpenSession())
            {
                return session.Query<Country>().Take(1000).ToList();
            }

        }
    }
}