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
    }
}
