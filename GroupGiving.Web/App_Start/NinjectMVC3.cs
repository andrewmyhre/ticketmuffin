using System.Configuration;
using System.Security.Principal;
using System.Web;
using EmailProcessing;
using EmailProcessing.Configuration;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.Web.Code;
using GroupGiving.Web.Controllers;
using Raven.Client;
using RavenDBMembership.Web.Models;

[assembly: WebActivator.PreApplicationStartMethod(typeof(GroupGiving.Web.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(GroupGiving.Web.App_Start.NinjectMVC3), "Stop")]

namespace GroupGiving.Web.App_Start
{
    using System.Reflection;
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
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IAccountService>().To<AccountService>();
            kernel.Bind<IEventService>().To<EventService>();
            kernel.Bind<IDocumentSession>()
                .ToMethod(delegate { return RavenDbDocumentStore.Instance.OpenSession(); })
                .InRequestScope();
            kernel.Bind<IDocumentStore>().ToMethod(x=>RavenDbDocumentStore.Instance);

            kernel.Bind<IRepository<GroupGivingEvent>>().To<RavenDBRepositoryBase<GroupGivingEvent>>();
            kernel.Bind<IRepository<Account>>().To<RavenDBRepositoryBase<Account>>();

            kernel.Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
            kernel.Bind<IMembershipService>().To<AccountMembershipService>();

            kernel.Bind<AccountController>().ToSelf();

            kernel.Bind<IEmailRelayService>().To<SimpleSmtpEmailRelayService>();
            kernel.Bind<IEmailFacade>().ToMethod((request) => MvcApplication.EmailFacade);
            kernel.Bind<IEmailPackageRelayer>()
                .ToMethod(x => EmailSenderFactory.CreateRelayerFromConfiguration(ConfigurationManager.GetSection("emailBuilder") as EmailBuilderConfigurationSection));
            kernel.Bind<ICountryService>().To<CountryService>();
            kernel.Bind<IPayPalConfiguration>().ToMethod(
                (request) => System.Configuration.ConfigurationManager.GetSection("paypal") as PayPalConfiguration);
            kernel.Bind<IApiClient>().ToMethod((request) =>
            {
                IPayPalConfiguration config = kernel.Get<IPayPalConfiguration>();
                return new ApiClient(new ApiClientSettings()
                {
                    Username = config.PayPalMerchantUsername,
                    Password = config.PayPalMerchantPassword,
                    Signature = config.PayPalMerchantSignature
                },
                                    config);
            });
            kernel.Bind<IPaymentGateway>().To<PayPalPaymentGateway>();
            kernel.Bind<ITaxAmountResolver>().To<NilTax>();
            kernel.Bind<IIdentity>().ToMethod(x=>HttpContext.Current.User.Identity);
            kernel.Bind<IContentProvider>().To<RavenDbContentProvider>();
            kernel.Bind<IMembershipProviderLocator>().To<RavenDbMembershipProviderLocator>();
            kernel.Bind<IPaypalAccountService>().To<PaypalAccountService>();
            kernel.Bind<PaypalAdaptiveAccountsConfigurationSection>().ToMethod(r => 
                ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection);
        }        
    }
}
