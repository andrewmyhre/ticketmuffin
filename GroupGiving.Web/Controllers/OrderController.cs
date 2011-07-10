using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;

        public OrderController()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        public ActionResult Index(int eventId)
        {
            return View();
        }

        public ActionResult PaymentRequest()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult StartRequest(PurchaseDetails purchaseDetails)
        {
            var viewModel = new OrderRequestViewModel();

            var eventDetails = _eventRepository.Retrieve(e => e.ShortUrl == purchaseDetails.ShortUrl);

            decimal amount = eventDetails.TicketPrice*purchaseDetails.Quantity;
            PayResponse response = SendPaymentRequest(amount);
            var account = _accountService.RetrieveByEmailAddress(purchaseDetails.EmailAddress);
            if (account==null)
            {
                IEmailRelayService emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
                _accountService.CreateIncompleteAccount(purchaseDetails.EmailAddress, emailRelayService);
            }

            viewModel.PayPalPostUrl = ConfigurationManager.AppSettings["PayFlowProPaymentPage"];
            viewModel.Ack = response.ResponseEnvelope.Ack;
            viewModel.PayKey = response.PayKey;
            viewModel.Errors = response.Errors;

            if (response.Errors != null && response.Errors.Count() > 0)
                return View(viewModel);

            return Redirect(string.Format(ConfigurationManager.AppSettings["PayFlowProPaymentPage"], response.PayKey));
        }

        private PayResponse SendPaymentRequest(decimal amount)
        {
            IApiClient payPal = new ApiClient(new ApiClientSettings()
                                                  {
                                                      Username = "seller_1304843436_biz_api1.gmail.com",
                                                      Password = "1304843443",
                                                      Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7"
                                                  });
            decimal amountCommissionAdded = amount*1.05m;
            var request = new PayRequest();
            request.ActionType = "PAY";
            request.CurrencyCode = "GBP";
            request.FeesPayer = "EACHRECEIVER";
            request.Memo = "test order";
            request.CancelUrl = "http://" + Request.Url.Authority + "/Order/Cancel?payKey=${payKey}.";
            request.ReturnUrl = "http://" + Request.Url.Authority + "/Order/Return?payKey=${payKey}.";
            request.Receivers.Add(new Receiver(amountCommissionAdded.ToString("#.00"), "seller_1304843436_biz@gmail.com"));
            request.Receivers.Add(new Receiver(amount.ToString("#.00"), "sellr2_1304843519_biz@gmail.com"));
            return payPal.SendPayRequest(request);
        }

        public ActionResult Return()
        {
            return View();
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
