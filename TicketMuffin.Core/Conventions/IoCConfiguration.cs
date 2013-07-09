using Ninject;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Conventions
{
    public class IoCConfiguration : NinjectModule
    {
        public override void Load()
        {
            Bind<IPledgeService>().To<PledgeService>();
            Bind<IPaymentService>().To<PaymentService>();
            Bind<IAuthenticationService>().To<AuthenticationService>();
            Bind<IAccountService>().To<AccountService>();
            Bind<IEventService>().To<EventService>();
            Bind<ICountryService>().To<CountryService>();
            Bind<ICurrencyStore>().To<CurrencyStore>();
        }
    }
}
