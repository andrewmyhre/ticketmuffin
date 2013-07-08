using System.Collections.Generic;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface ICurrencyStore
    {
        Currency GetCurrencyByIso4217Code(int iso4217Code);
        Currency GetCurrencyByIso4217Code(string iso4217Code);
        IEnumerable<Currency> AllCurrencies();
    }
}