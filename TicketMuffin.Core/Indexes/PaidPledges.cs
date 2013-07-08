using System.Linq;
using Raven.Client.Indexes;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Indexes
{
    public class PaidPledges : AbstractIndexCreationTask<EventPledge,PaidPledges.EventPledgePayment>
    {
        public class EventPledgePayment
        {
            public string OrderNumber { get; set; }
            public string AccountEmailAddress { get; set; }
            public decimal Total { get; set; }
        }
        public PaidPledges()
        {
            Map = pledges => from pledge in pledges
                             from payment in Recurse(pledge, x=>x.Payments.Where(p=>p.PaymentStatus == PaymentStatus.AuthorisedUnsettled || p.PaymentStatus == PaymentStatus.Settled))
                             select new
                                 {
                                     OrderNumber = pledge.OrderNumber,
                                     AccountEmailAddress = pledge.AccountEmailAddress,
                                     Total = pledge.Total,
                                     PaymentStatus = payment.PaymentStatus,
                                     Amount = payment.Total,
                                     CurrencyCode = payment.SettlementCurrencyCode,
                                     ModifiedDate = payment.ModifiedDate
                                 };
        }
    }
}