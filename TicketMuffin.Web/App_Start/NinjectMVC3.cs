using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using EmailProcessing;
using EmailProcessing.Configuration;
using Ninject.Activation;
using Ninject.Parameters;
using Raven.Client;
using TicketMuffin.Core.Actions;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.App_Start;
using TicketMuffin.Web.Code;
using TicketMuffin.Web.Controllers;
using TicketMuffin.Web.Models;
using log4net;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NinjectMVC3), "Stop")]

namespace TicketMuffin.Web.App_Start
{
    public static class NinjectMVC3 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            ServiceLocator.Instance = kernel;
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IContentProvider>()
                .To<RavenDbContentProvider>()
                .InRequestScope();
            kernel.Bind<ILog>().ToMethod(
                delegate(IContext request)
                    {
                        if (request.Request.Target != null)
                            return LogManager.GetLogger(request.Request.Target.Member.DeclaringType.Name);

                        return LogManager.GetLogger(typeof (NinjectMVC3));
                    });

            kernel.Bind<IAccountService>().To<AccountService>();
            kernel.Bind<IEventService>().To<EventService>();

            kernel.Bind<IDocumentStore>()
                .ToMethod(ctx =>
                              {
                                  var documentStore = new Raven.Client.Document.DocumentStore()
                                                          {
                                                              Url = "http://localhost:8080"
                                                          };
                                  documentStore.Initialize();
                                  RavenDbIndexes.Initialise(documentStore);
                                  return documentStore;
                              })
                .InSingletonScope();

            kernel.Bind<IDocumentSession>()
                .ToMethod(ctx=>kernel.Get<IDocumentStore>().OpenSession())
                .InRequestScope()
                .OnDeactivation((ctx,session)=>
                                    {
                                        session.SaveChanges();
                                    });

            kernel.Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
            kernel.Bind<IMembershipService>().To<AccountMembershipService>();

            kernel.Bind<AccountController>().ToSelf();

            kernel.Bind<IEmailRelayService>().To<SimpleSmtpEmailRelayService>();
            kernel.Bind<IEmailTemplateManager>().ToMethod(ctx =>
                                                              {
                                                                  var configuration =
                                                                      kernel.Get<EmailBuilderConfigurationSection>();
                                                                  var templateManager =
                                                                      new EmailTemplateManager(
                                                                          configuration.TemplateLocation);
                                                                  templateManager.LoadTemplates();
                                                                  return templateManager;
                                                              });
            kernel.Bind<EmailBuilderConfigurationSection>()
                .ToMethod(ctx => ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection);
            kernel.Bind<IEmailFacade>().ToMethod((request) =>
                                                     {
                                                         var configuration =
                                                             kernel.Get<EmailBuilderConfigurationSection>();
                                                         var service = new EmailFacade(configuration,
                                                             Activator.CreateInstance(Type.GetType(configuration.EmailSenderType), configuration) as IEmailPackageRelayer,
                                                             kernel.Get<IEmailTemplateManager>());
                                                         service.LoadTemplates();

                                                         return service;
                                                     }
                );
            kernel.Bind<IEmailPackageRelayer>()
                .ToMethod(x => EmailSenderFactory.CreateRelayerFromConfiguration(ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection));
            kernel.Bind<ICountryService>().To<CountryService>();
            kernel.Bind<ISiteConfigurationService>().To<SiteConfigurationService>();
            kernel.Bind<ISiteConfiguration>()
                .ToMethod(r => kernel.Get<ISiteConfigurationService>().GetConfiguration())
                .InRequestScope();
            kernel.Bind<IApiClient>().ToMethod((request) =>
            {
                var configuration = kernel.Get<ISiteConfiguration>();
                return new ApiClient(new ApiClientSettings(configuration.AdaptiveAccountsConfiguration));
            });
            kernel.Bind<IPaymentGateway>().To<PayPalPaymentGateway>();
            kernel.Bind<ITaxAmountResolver>().To<NilTax>();
            kernel.Bind<IIdentity>().ToMethod(x=>HttpContext.Current.User.Identity);
            kernel.Bind<MembershipProvider>()
                .ToMethod(ctx=>
                              {
                                  var provider =
                                      System.Web.Security.Membership.Provider as
                                      RavenDBMembership.Provider.RavenDBMembershipProvider;
                                  provider.DocumentStore = kernel.Get<IDocumentStore>();
                                  provider.ApplicationName = "TicketMuffin";
                                  return provider;
                              });
            kernel.Bind<RoleProvider>()
                .ToMethod(ctx =>
                              {
                                  var provider = System.Web.Security.Roles.Provider
                                                 as RavenDBMembership.Provider.RavenDBRoleProvider;
                                  provider.DocumentStore = kernel.Get<IDocumentStore>();
                                  provider.ApplicationName = "TicketMuffin";
                                  return provider;
                              });
            kernel.Bind<PaypalAdaptiveAccountsConfigurationSection>().ToMethod(r => 
                ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection);
            kernel.Bind<AdaptiveAccountsConfiguration>().ToMethod(r=>kernel.Get<ISiteConfiguration>().AdaptiveAccountsConfiguration);
            kernel.Bind<IPayRequestFactory>().To<PayRequestFactory>();

            kernel.Bind<IWindowsService>().To<SiteWarmupService>()
                .WithParameter(new ConstructorArgument("remoteUrl", ConfigurationManager.AppSettings["warmupUrl"]));
            kernel.Bind<ICultureService>().To<CultureService>().InRequestScope();
            kernel.Bind<ApplicationDataSetup>().ToSelf();
            kernel.Bind<ITicketGenerator>().To<TicketGenerator>();
            kernel.Bind<IPledgeTicketSender>().To<PledgeTicketSender>();
            kernel.Bind<IEventCultureResolver>().To<EventCultureResolver>();
        }        
    }
}
