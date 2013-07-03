using System;
using System.Configuration;
using EmailProcessing;
using EmailProcessing.Configuration;
using Ninject;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.Code;

namespace TicketMuffin.Web.Conventions
{
    internal class TicketMuffinWebNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>().ToMethod(x => { var store = new DocumentStore() {Url = "http://localhost:8080"};
                                                     store.Initialize();
                                                     return store;
            }).InSingletonScope();
            Bind<IDocumentSession>().ToMethod(x => Kernel.Get<IDocumentStore>().OpenSession()).OnDeactivation(x => x.Dispose());
            
            BindEmailRelatedThings();

            Bind<ICultureService>().To<CultureService>();
            Bind<IContentProvider>().To<RavenDbContentProvider>();
        }

        private void BindEmailRelatedThings()
        {
// email stuff
            Bind<IEmailRelayService>().To<SimpleSmtpEmailRelayService>();
            Bind<IEmailTemplateManager>().ToMethod(ctx =>
                {
                    var configuration =
                        Kernel.Get<EmailBuilderConfigurationSection>();
                    var templateManager =
                        new EmailTemplateManager(
                            configuration.TemplateLocation);
                    templateManager.LoadTemplates();
                    return templateManager;
                });
            Bind<EmailBuilderConfigurationSection>()
                .ToMethod(ctx => ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection);
            Bind<IEmailFacade>().ToMethod((request) =>
                {
                    var configuration =
                        Kernel.Get<EmailBuilderConfigurationSection>();
                    var service = new EmailFacade(configuration,
                                                  Activator.CreateInstance(Type.GetType(configuration.EmailSenderType),
                                                                           configuration) as IEmailPackageRelayer,
                                                  Kernel.Get<IEmailTemplateManager>());
                    service.LoadTemplates();

                    return service;
                }
                );
            Bind<IEmailPackageRelayer>()
                .ToMethod(
                    x =>
                    EmailSenderFactory.CreateRelayerFromConfiguration(
                        ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection));
        }
    }
}