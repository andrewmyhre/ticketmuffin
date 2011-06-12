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
            throw new NotImplementedException();
        }
    }
}
