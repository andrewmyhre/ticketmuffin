using System;
using System.Collections;
using System.Collections.Generic;

namespace GroupGiving.Core.Services
{
    public interface ICountryService
    {
        IEnumerable<Country> RetrieveAllCountries();
    }
}