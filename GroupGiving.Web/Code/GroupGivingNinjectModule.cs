using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Ninject.Modules;
using Raven.Client;

namespace GroupGiving.Web.Code
{
    public class GroupGivingNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserService>().To<UserService>();
            Bind<IEventService>().To<EventService>();
            Bind<IDocumentSession>()
                .ToMethod(delegate { return RavenDbDocumentStore.Instance.OpenSession(); })
                .InRequestScope();

            Bind<IRepository<GroupGivingEvent>>().To<Core.Data.Fakes.FakeRepository<GroupGivingEvent>>();
        }
    }
}