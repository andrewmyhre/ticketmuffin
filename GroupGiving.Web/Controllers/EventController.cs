using System;
using System.Web.Security;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using System.Web.Mvc;
using Ninject;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;

        public EventController()
        {
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        //
        // GET: /Event/
        public ActionResult Index(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new EventViewModel();
            var givingEvent = _eventRepository.Retrieve(e=>e.ShortUrl==shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            viewModel.EventId = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefits = givingEvent.AdditionalBenefits;
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.Description = givingEvent.Description;
            viewModel.IsFeatured = givingEvent.IsFeatured;
            viewModel.IsPrivate = givingEvent.IsPrivate;
            viewModel.MaximumParticipants = givingEvent.MaximumParticipants;
            viewModel.MinimumParticipants = givingEvent.MinimumParticipants;
            viewModel.PaypalAccountEmailAddress = givingEvent.PaypalAccountEmailAddress;
            viewModel.PhoneNumber = givingEvent.PhoneNumber;
            viewModel.SalesEndDateTime = givingEvent.SalesEndDateTime;
            viewModel.ShortUrl = givingEvent.ShortUrl;
            viewModel.Title = givingEvent.Title;
            viewModel.TicketPrice = givingEvent.TicketPrice;
            viewModel.Venue = givingEvent.Venue;

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("share")]
        public ActionResult Share(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new ShareEventViewModel();
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("pledge")]
        public ActionResult Pledge(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new EventPledgeViewModel();
            var givingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            viewModel.EventId = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefits = givingEvent.AdditionalBenefits;
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.Description = givingEvent.Description;
            viewModel.IsFeatured = givingEvent.IsFeatured;
            viewModel.IsPrivate = givingEvent.IsPrivate;
            viewModel.MaximumParticipants = givingEvent.MaximumParticipants;
            viewModel.MinimumParticipants = givingEvent.MinimumParticipants;
            viewModel.PaypalAccountEmailAddress = givingEvent.PaypalAccountEmailAddress;
            viewModel.PhoneNumber = givingEvent.PhoneNumber;
            viewModel.SalesEndDateTime = givingEvent.SalesEndDateTime;
            viewModel.ShortUrl = givingEvent.ShortUrl;
            viewModel.Title = givingEvent.Title;
            viewModel.TicketPrice = givingEvent.TicketPrice;
            viewModel.Venue = givingEvent.Venue;

            if (User.Identity.IsAuthenticated)
            {
                var account = _accountService.RetrieveByEmailAddress(User.Identity.Name);
                viewModel.UserFirstName = account.FirstName;
                viewModel.UserLastName = account.LastName;
                viewModel.UserPayPalEmail = account.Email;
            }

            return View(viewModel);
        }
    }
}
