using Ninject;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Embedded;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Test.Integration
{
    public class TestNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>().ToMethod(x =>
                {
                    var store = new EmbeddableDocumentStore() {RunInMemory = true};
                    store.Initialize();
                    return store;
                }).InSingletonScope();
            Bind<IDocumentSession>().ToMethod(x => Kernel.Get<IDocumentStore>().OpenSession())
                                    .OnDeactivation(x => x.Dispose());
        }
    }
}