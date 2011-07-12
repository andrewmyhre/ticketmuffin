using System;
using System.Collections.Generic;
using System.Text;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Data
{
    public interface IEventPledgeRepository : IRepository<EventPledge>
    {
        EventPledge RetrieveByPayKey(string payKey);
        IEnumerable<EventPledge> RetrieveByEvent(string eventId);
    }
}
