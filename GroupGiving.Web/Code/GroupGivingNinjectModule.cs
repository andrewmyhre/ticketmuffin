using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.Web.Controllers;
using Ninject.Modules;
using Raven.Client;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Code
{
    public class GroupGivingNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAccountService>().To<AccountService>();
            Bind<IEventService>().To<EventService>();
            Bind<IDocumentSession>()
                .ToMethod(delegate { return RavenDbDocumentStore.Instance.OpenSession(); })
                .InRequestScope();

            Bind<IRepository<GroupGivingEvent>>().To<Core.Data.RavenDBRepositoryBase<GroupGivingEvent>>();
            Bind<IRepository<Account>>().To<Core.Data.RavenDBRepositoryBase<Account>>();
            Bind<IEventPledgeRepository>().To<EventPledgeRepository>();

            Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
            Bind<IMembershipService>().To<AccountMembershipService>();

            Bind<AccountController>().ToSelf();

            Bind<IEmailRelayService>().To<SimpleSmtpEmailRelayService>();
            Bind<IEmailCreationService>().To<EmailCreationService>();
            Bind<ICountryService>().To<CountryService>();
        }
    }
}