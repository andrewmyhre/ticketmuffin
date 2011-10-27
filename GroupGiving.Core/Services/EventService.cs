using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;

        public EventService(IRepository<GroupGivingEvent> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public IEnumerable<GroupGivingEvent> RetrieveAllEvents()
        {
            return _eventRepository.RetrieveAll();
        }

        public CreateEventResult CreateEvent(CreateEventRequest request)
        {
            GroupGivingEvent ggEvent = new GroupGivingEvent()
            {
                Title = request.Title,
                Description = request.Description,
                City = request.City,
                StartDate = request.StartDateTime,
                Venue = request.Venue,
                AddressLine = request.AddressLine,
                ShortUrl = request.ShortUrl,
                IsPrivate = request.IsPrivate,
                IsFeatured = request.IsFeatured,
                PhoneNumber = request.PhoneNumber,
                OrganiserId = request.OrganiserAccountId,
                State = EventState.Creating
            };
            
            _eventRepository.SaveOrUpdate(ggEvent);
            _eventRepository.CommitUpdates();

            int eventId = int.Parse(ggEvent.Id.Substring(ggEvent.Id.IndexOf('/') + 1));
            return new CreateEventResult() {Success = true, Event = ggEvent};
        }

        public void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            var ggEvent =
                _eventRepository.Retrieve(e => e.ShortUrl == setTicketDetailsRequest.ShortUrl);

            if (ggEvent==null)
                throw new ArgumentException("Event not found");

            ggEvent.TicketPrice = setTicketDetailsRequest.TicketPrice.Value;
            ggEvent.MinimumParticipants = setTicketDetailsRequest.MinimumParticipants.Value;
            ggEvent.MaximumParticipants = setTicketDetailsRequest.MaximumParticipants;
            ggEvent.SalesEndDateTime = setTicketDetailsRequest.SalesEndDateTime;
            ggEvent.AdditionalBenefits = setTicketDetailsRequest.AdditionalBenefits;
            ggEvent.PaypalAccountEmailAddress = setTicketDetailsRequest.PayPalEmail;
            ggEvent.PayPalAccountFirstName = setTicketDetailsRequest.PayPalFirstName;
            ggEvent.PayPalAccountLastName = setTicketDetailsRequest.PayPalLastName;
            ggEvent.State = EventState.SalesReady;

            _eventRepository.SaveOrUpdate(ggEvent);
            _eventRepository.CommitUpdates();
        }

        public bool ShortUrlAvailable(string shortUrl)
        {
            var ggEvent = _eventRepository.Retrieve(
                e => e.ShortUrl == shortUrl);
            return ggEvent == null;
        }

        public GroupGivingEvent Retrieve(int eventId)
        {
            return _eventRepository.Retrieve(e => e.Id == string.Format("groupgivingevents/{0}",eventId));
        }

        public GroupGivingEvent Retrieve(string shortUrl)
        {
            return _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
        }

        public void SendEventInvitationEmails(IEmailPackageRelayer emailPackageRelayer, string recipients, string body, string subject)
        {
            EmailPackage package = new EmailPackage();
            package.Subject = subject;
            package.Text = body;
            package.From = "noreply@ticketmuffin.com";

            string[] recipientList = recipients.Split(',');
            foreach (var recipient in recipientList)
            {
                package.To.Add(recipient.Trim());
            }
            emailPackageRelayer.Relay(package);
        }

    }
}
