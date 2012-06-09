using System.Linq;
using Raven.Client.Indexes;
using TicketMuffin.Core.Domain;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.App_Start
{
    public class DenormalizedPledges : IndexDefinitionBuilder<GroupGivingEvent,TransactionHistoryItem>
    {
        public DenormalizedPledges()
        {
            Map = events => from e in events
                             from pledge in e.Pledges
                             from attendee in pledge.Attendees
                             select new
                             {
                                 EventId = e.Id,
                                 EventName=e.Title,
                                 EventOrganiser=e.OrganiserName,
                                 OrderNumber = pledge.OrderNumber,
                                 TransactionId = pledge.TransactionId,
                                 AccountEmailAddress = pledge.AccountEmailAddress,
                                 PaymentStatus = pledge.PaymentStatus,
                                 AttendeeName = attendee.FullName
                             };

            Reduce = results => from result in results
                                group result by result.OrderNumber
                                into g
                                select new
                                           {
                                               OrderNumber = g.Key,
                                               EventId = g.First().EventId,
                                               EventName = g.First().EventName,
                                               EventOrganiser = g.First().EventOrganiser,
                                               TransactionId = g.First().TransactionId,
                                               AccountEmailAddress = g.First().AccountEmailAddress,
                                               PaymentStatus = g.First().PaymentStatus,
                                               AttendeeName = g.First().AttendeeName
                                           };

            TransformResults = (database, results) =>
                               from result in results
                               select new
                                          {
                                              OrderNumber = result.OrderNumber,
                                              TransactionId = result.TransactionId,
                                              EventId = result.EventId,
                                              EventName=result.EventName,
                                              EventOrganiser=result.EventOrganiser,
                                              AccountEmailAddress = result.AccountEmailAddress,
                                              PaymentStatus = result.PaymentStatus,
                                              AttendeeName = result.AttendeeName
                                          };
        }
    }
}