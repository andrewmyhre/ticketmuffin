using System.Collections.Generic;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Services
{
    public interface IPaymentService
    {
        string GetPaymentUrlForPledge(GroupGivingEvent @event, Account eventOrganiser, EventPledge pledge);
        PaymentDetailsResponse GetPaymentDetails(Payment payment);
    }
}