namespace TicketMuffin.Core.Services
{
    public class NilTax : ITaxAmountResolver
    {
        public decimal LookupTax()
        {
            return 0m;
        }

        public decimal LookupTax(string country)
        {
            return 0m;
        }
    }
}