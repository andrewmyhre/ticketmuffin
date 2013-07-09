using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class CurrencyStore : ICurrencyStore
    {
        private readonly IDocumentSession _session;

        public CurrencyStore(IDocumentSession session)
        {
            _session = session;
        }

        public Currency GetCurrencyByIso4217Code(int iso4217Code)
        {
            return _session.Load<Currency>("currencies/" + iso4217Code);
        }

        public Currency GetCurrencyByIso4217Code(string iso4217Code)
        {
            return _session.Query<Currency>().SingleOrDefault(c => c.Iso4217AlphaCode == iso4217Code);
        }

        public IEnumerable<Currency> AllCurrencies()
        {
            return _session.Query<Currency>().ToArray();
        }

        public void CreateDefaults()
        {
            //if (_session.Load<Currency>("currencies/985")==null)
                _session.Store(new Currency(){Iso4217NumericCode= 985, Iso4217AlphaCode = "PLN"});
            //if (_session.Load<Currency>("currencies/826") == null)
                _session.Store(new Currency() { Iso4217NumericCode = 826, Iso4217AlphaCode = "GBP" });
            //if (_session.Load<Currency>("currencies/978") == null)
                _session.Store(new Currency() { Iso4217NumericCode = 978, Iso4217AlphaCode = "EUR" });
            _session.SaveChanges();
        }
    }
}