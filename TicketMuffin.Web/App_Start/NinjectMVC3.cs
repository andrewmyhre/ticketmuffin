using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using EmailProcessing;
using EmailProcessing.Configuration;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Clients;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Controllers;
using Ninject.Activation;
using Raven.Client;
using RavenDBMembership.Web.Models;
using log4net;

[assembly: WebActivator.PreApplicationStartMethod(typeof(GroupGiving.Web.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(GroupGiving.Web.App_Start.NinjectMVC3), "Stop")]

namespace GroupGiving.Web.App_Start
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Mvc;

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
                                        var request = ctx.GetScope() as HttpRequestBase;
                                        if (request != null)
                                        {
                                            if (request.RequestContext.HttpContext.Error == null)
                                            {
                                                session.SaveChanges();
                                            }
                                        }
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
                                                              }).InSingletonScope();
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
                ).InSingletonScope();
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

            kernel.Bind<IWindowsService>().To<SiteWarmupService>();
            kernel.Bind<ICultureService>().To<CultureService>().InRequestScope();
            kernel.Bind<ApplicationDataSetup>().ToSelf();
        }        
    }
}