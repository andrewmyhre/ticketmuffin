using GroupGiving.Core.Domain;
using GroupGiving.PayPal.Configuration;

namespace GroupGiving.Core.Configuration
{
    public class SiteConfiguration : ISiteConfiguration
    {
        public string Id { get; set; }
        public string EventImagePathFormat { get; set; }
        public string LoginUrl { get; set; }
        public DatabaseConfiguration DatabaseConfiguration { get; set; }
        public JustGivingApiConfiguration JustGivingApiConfiguration { get; set; }
        public AdaptiveAccountsConfiguration AdaptiveAccountsConfiguration { get; set; }
   }
}
