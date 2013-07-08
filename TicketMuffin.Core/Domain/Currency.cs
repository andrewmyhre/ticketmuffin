using System.Globalization;

namespace TicketMuffin.Core.Domain
{
    public class Currency
    {
        public string Iso4217AlphaCode { get; set; }
        public int Iso4217NumericCode { get; set; }
        public string Id { get; set; }
    }
}
