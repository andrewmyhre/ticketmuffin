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
            CultureInfo culture = new CultureInfo("fr-FR");
            switch(currency)
            {
                case Currency.PLN:
                    culture = new CultureInfo("pl-PL");
                    break;
                case Currency.GBP:
                    culture = new CultureInfo("en-GB");
                    break;
                case Currency.USD:
                    culture = new CultureInfo("en-US");
                    break;
                case Currency.EUR:
                default:
                   culture = new CultureInfo("fr-FR", false);
                   break;
            }
            return culture;
        }
    }
}
