using System.Collections.Generic;
using Raven.Client;

namespace TicketMuffin.Core.Services
{
    public interface ICountryService
    {
        IEnumerable<Country> RetrieveAllCountries();
        IEnumerable<Country> LoadCountriesFromCsv(IDocumentSession session, string sourceFilePath);
        void EnsureCountryData(string countryDataSourceFile);
    }
}