using System;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Models;
using System.Web.Mvc;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        public EventController()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
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

            var viewModel = new EventViewModel();
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

            return View(viewModel);
        }
    }
}
