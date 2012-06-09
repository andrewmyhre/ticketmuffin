using TicketMuffin.PayPal.Configuration;

namespace TicketMuffin.Core.Configuration
{
    public interface ISiteConfiguration
    {
        string Id { get; set; }
        string EventImagePathFormat { get; set; }
        string LoginUrl { get; set; }
        DatabaseConfiguration DatabaseConfiguration { get; set; }
        JustGivingApiConfiguration JustGivingApiConfiguration { get; set; }
        AdaptiveAccountsConfiguration AdaptiveAccountsConfiguration { get; set; }
    }
}