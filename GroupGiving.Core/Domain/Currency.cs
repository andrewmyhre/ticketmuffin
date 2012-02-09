using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GroupGiving.Core.Domain
{
    public enum Currency
    {
        GBP,
        PLN,
        EUR,
        USD
    }

    public static class CurrencyExtensions
    {
        public static CultureInfo AsCulture(this Currency currency)
        {
            switch(currency)
            {
                case Currency.PLN:
                    return new CultureInfo("pl-PL");
                case Currency.GBP:
                    return new CultureInfo("en-GB");
                case Currency.USD:
                    return new CultureInfo("en-US");
                case Currency.EUR:
                default:
                   return new CultureInfo("fr-FR", false);
            }
        }
    }
}
