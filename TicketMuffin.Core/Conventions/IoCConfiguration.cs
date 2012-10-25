using Ninject;
using Raven.Client;

namespace TicketMuffin.Core.Conventions
{
    public class IoCConfiguration
    {
        public static void BindAll(IKernel kernel)
        {
            kernel.Bind<IDocumentStore>()
                .ToMethod(ctx =>
                {
                    return RavenStore.CreateDocumentStore();
                })
                .InSingletonScope();

            kernel.Bind<IDocumentSession>()
                .ToMethod(ctx => kernel.Get<IDocumentStore>().OpenSession())
                .InRequestScope()
                .OnDeactivation((ctx, session) => session.SaveChanges());
        }
    }
}
