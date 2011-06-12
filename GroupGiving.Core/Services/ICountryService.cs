using System;
using System.Collections;
using System.Collections.Generic;

namespace GroupGiving.Core.Services
{
    public interface ICountryService
    {
        IEnumerable<Country> RetrieveAllCountries();
    }

    public class CountryService : ICountryService
    {
        public IEnumerable<Country> RetrieveAllCountries()
        {
            return new[]
                       {
                           new Country("Poland"),
                           new Country("United Kingdom"), 
                           new Country("United States of America"),
                           new Country("New Zealand")
                       };
        }
    }

    public class Country
    {
        public Country(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}