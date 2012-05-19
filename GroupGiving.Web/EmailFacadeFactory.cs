using System.Configuration;
using EmailProcessing;
using EmailProcessing.Configuration;

namespace GroupGiving.Web
{
    public class EmailFacadeFactory
    {
        public EmailFacade CreateFromConfiguration()
        {
            return new EmailFacade(ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection);
        }
    }
}