using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Services
{
    public class PledgeService : IPledgeService
    {
        private readonly IDocumentSession _session;

        public PledgeService(IDocumentSession session)
        {
            _session = session;
        }

        public void PurchaseTicket(string pledgeId, string payerId)
        {
            throw new NotImplementedException();
        }

        public EventPledge CreatePledge(GroupGivingEvent @event, Account pledger)
        {
            var pledge = new EventPledge()
                {
                    AccountId = pledger.Id,
                    AccountEmailAddress = pledger.Email,
                    AccountName = string.Concat(" ", pledger.FirstName, pledger.LastName),
                    DatePledged = DateTime.Now
                };
            @event.Pledges.Add(pledge);
            _session.SaveChanges();
            return pledge;
        }
    }

    public interface IPledgeService
    {
        void PurchaseTicket(string pledgeId, string payerId);
        EventPledge CreatePledge(GroupGivingEvent @event, Account pledger);
    }
}
