using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Core.Data
{
    public class EventPledgeRepository : RavenDBRepositoryBase<EventPledge>, IEventPledgeRepository
    {
        public EventPledgeRepository(IDocumentSession session) : base(session)
        {
        }

        public EventPledge RetrieveByPayKey(string payKey)
        {
            return _session.Query<EventPledge>().Where(ep => ep.PayPalPayKey == payKey).SingleOrDefault();
        }

        public IEnumerable<EventPledge> RetrieveByEvent(string eventId)
        {
            return _session.Query<EventPledge>().Where(ep => ep.EventId == eventId);
        }
    }
}