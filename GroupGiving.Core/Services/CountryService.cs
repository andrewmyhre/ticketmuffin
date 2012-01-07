using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Com.StellmanGreene.CSVReader;
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

        public IEnumerable<Country> LoadCountriesFromCsv(IDocumentSession session, string sourceFilePath)
        {
            List<Country> loadedCountries = new List<Country>();
            using (var filestream = System.IO.File.OpenRead(sourceFilePath))
            using (var reader = new StreamReader(filestream))
            {
                var countries = session.Query<Country>().ToList();
                foreach (var country in countries)
                {
                    session.Delete(country);
                }
                session.SaveChanges();

                CSVReader csv = new CSVReader(reader);
                var table = csv.CreateDataTable(true);
                foreach (DataRow row in table.Rows)
                {
                    var country = new Country((string)row["Common Name"]);
                    session.Store(country);
                    loadedCountries.Add(country);
                }
                session.SaveChanges();
            }

            return loadedCountries;
        }

        public void EnsureCountryData(string countryDataSourceFile)
        {
            using (var session = _store.OpenSession())
            {
                var countries = session.Query<Country>().Count();
                if (countries == 0)
                {
                    LoadCountriesFromCsv(session, countryDataSourceFile);
                }
            }
        }
    }
}