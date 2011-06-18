using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                PhoneNumber = request.PhoneNumber
            };

            _eventRepository.SaveOrUpdate(ggEvent);
            _eventRepository.CommitUpdates();

            int eventId = int.Parse(ggEvent.Id.Substring(ggEvent.Id.IndexOf('/') + 1));
            return new CreateEventResult() {Success = true, EventId = eventId};
        }

        public void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            var ggEvent =
                _eventRepository.Retrieve(
                    e => e.Id == string.Format("groupgivingevents/{0}", setTicketDetailsRequest.EventId));

            if (ggEvent==null)
                throw new ArgumentException("Event not found");

            ggEvent.TicketPrice = setTicketDetailsRequest.TicketPrice;
            ggEvent.MinimumParticipants = setTicketDetailsRequest.MinimumParticipants;
            ggEvent.MaximumParticipants = setTicketDetailsRequest.MaximumParticipants;
            ggEvent.SalesEndDateTime = setTicketDetailsRequest.SalesEndDateTime;
            ggEvent.AdditionalBenefits = setTicketDetailsRequest.AdditionalBenefits;
            ggEvent.PaypalAccountEmailAddress = setTicketDetailsRequest.PaypalAccountEmailAddress;

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
    }
}
