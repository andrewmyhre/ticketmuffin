using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using EmailProcessing;
using EmailProcessing.Configuration;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using TicketMuffin.Core.Actions;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.Web.Code;
using Raven.Client.Extensions;
using TicketMuffin.Web.Models;
using TicketMuffin.Web.Services;
using log4net;

namespace TicketMuffin.Web.Conventions
{
    internal class TicketMuffinWebNinjectModule : NinjectModule
    {
        private const string DATABASE_NAME = "TicketMuffin";
        private ILog _logger = LogManager.GetLogger(typeof (TicketMuffinWebNinjectModule));
        public override void Load()
        {
            Bind<IDocumentStore>().ToMethod(x => { 
                var store = new DocumentStore() {Url = "http://localhost:8080"};
                store.Initialize();
                store.DatabaseCommands.EnsureDatabaseExists(DATABASE_NAME);

                store.Conventions.RegisterIdConvention<LocalisedContent>((dbname, commands, content) => string.Join("/","content", content.Culture, content.Address, content.Label));

                var catalog = new CompositionContainer(new AssemblyCatalog(Assembly.GetAssembly(typeof(TicketMuffin.Core.Indexes.ContentByCultureAndAddress))));
                var databaseCommands = store.DatabaseCommands.ForDatabase(DATABASE_NAME);
                IndexCreation.CreateIndexes(catalog, databaseCommands, store.Conventions);

                return store;
            }).InSingletonScope();
            Bind<IDocumentSession>()
                .ToMethod(x => Kernel.Get<IDocumentStore>().OpenSession(DATABASE_NAME))
                .InRequestScope()
                .OnActivation((context,session)=>_logger.Debug("Activating a doc session for " + context.GetScope()))
                .OnDeactivation((context,session) =>
                    {
                        if (session.Advanced.HasChanges)
                        {
                            _logger.Debug("session has unsaved changes");
                        }

                        session.Dispose();
                        _logger.Debug("Deactivating a document session for " + context.GetScope());
                    });
            
            BindEmailRelatedThings();

            Bind<ICultureService>().To<CultureService>();
            Bind<IIdentity>().ToMethod(x => HttpContext.Current.User.Identity);
            Bind<IPayPalApiClient>().To<PayPalApiClient>();
            Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
            Bind<IContentProvider>().To<RavenDbContentProvider>().InRequestScope();
            Bind<ISiteConfigurationService>().To<SiteConfigurationService>();
            Bind<AdaptiveAccountsConfiguration>()
                .ToMethod(x =>
                    {
                        var siteConfig = Kernel.Get<ISiteConfigurationService>();
                        return siteConfig.GetConfiguration("Sandbox").AdaptiveAccountsConfiguration;
                    });
            Bind<IPaymentGateway>().To<FakePaymentGateway>();
            Bind<ITaxAmountResolver>().To<NilTax>();
            Bind<ITicketGenerator>().To<TicketGenerator>()
                .WithConstructorArgument("ticketTemplatePath", System.Web.Hosting.HostingEnvironment.MapPath("~/Content/tickets/ticket-pl.pdf"));
            Bind<IEventCultureResolver>().To<EventCultureResolver>();
            Bind<IOrderNumberGenerator>().To<OrderNumberGenerator>();
            Bind<IPledgeTicketSender>().To<PledgeTicketSender>();
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