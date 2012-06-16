using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface ITicketGenerator
    {
        void CreateTicket(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture);
    Stream LoadTicket(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture);
    }
}
