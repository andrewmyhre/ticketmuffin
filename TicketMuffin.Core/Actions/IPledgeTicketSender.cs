using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Actions
{
    public interface IPledgeTicketSender
    {
        void SendTickets(GroupGivingEvent @event, EventPledge pledge);
    }
}
