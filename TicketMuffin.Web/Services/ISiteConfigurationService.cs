using TicketMuffin.Web.Configuration;

namespace TicketMuffin.Web.Services
{
    public interface ISiteConfigurationService
    {
        ISiteConfiguration GetConfiguration();
        void EnsureConfigurationData();
    }
}