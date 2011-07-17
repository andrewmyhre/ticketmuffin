using EmailProcessing;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.Web.Controllers;
using Ninject;
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

            Bind<IRepository<GroupGivingEvent>>().To<RavenDBRepositoryBase<GroupGivingEvent>>();
            Bind<IRepository<Account>>().To<RavenDBRepositoryBase<Account>>();

            Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
            Bind<IMembershipService>().To<AccountMembershipService>();

            Bind<AccountController>().ToSelf();

            Bind<IEmailRelayService>().To<SimpleSmtpEmailRelayService>();
            Bind<IEmailFacade>().ToMethod((request) => EmailFacadeFactory.CreateFromConfiguration());
            Bind<ICountryService>().To<CountryService>();
            Bind<IPayPalConfiguration>().ToMethod(
                (request) => System.Configuration.ConfigurationManager.GetSection("paypal") as PayPalConfiguration);
            Bind<IApiClient>().ToMethod((request)=>
                {
                    IPayPalConfiguration config = MvcApplication.Kernel.Get<IPayPalConfiguration>();
                    return new ApiClient(new ApiClientSettings()
                                        {
                                            Username = config.PayPalMerchantUsername,
                                            Password = config.PayPalMerchantPassword,
                                            Signature = config.PayPalMerchantSignature
                                        },
                                        config);
                });
            Bind<IPaymentGateway>().To<PayPalPaymentGateway>();
            Bind<ITaxAmountResolver>().To<NilTax>();
        }
    }
}