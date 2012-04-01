using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public interface ISiteConfigurationService
    {
        ISiteConfiguration GetConfiguration();
        void EnsureConfigurationData();
    }
}