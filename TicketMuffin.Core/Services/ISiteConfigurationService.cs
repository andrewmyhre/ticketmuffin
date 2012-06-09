using TicketMuffin.Core.Configuration;

namespace TicketMuffin.Core.Services
{
    public interface ISiteConfigurationService
    {
        ISiteConfiguration GetConfiguration();
        void EnsureConfigurationData();
    }
}