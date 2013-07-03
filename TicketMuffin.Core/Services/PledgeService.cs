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
        private readonly IPaymentService _paymentService;

        public PledgeService(IDocumentSession session, IPaymentService paymentService)
        {
            _session = session;
            _paymentService = paymentService;
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

        public void ConfirmPaidPledge(GroupGivingEvent @event, EventPledge pledge, Account pledger, string transactionId)
        {
            var payment = pledge.Payments.SingleOrDefault(x => x.TransactionId == transactionId);
            if (payment == null)
            {
                throw new ArgumentException("Payment could not be found");
            }

            var gatewayDetails = _paymentService.GetPaymentDetails(payment);
            if (gatewayDetails.PaymentStatus == PaymentStatus.Unsettled
                || payment.PaymentStatus == PaymentStatus.Settled)
            {
                payment.PaymentStatus = gatewayDetails.PaymentStatus;
                pledger.PaymentGatewayId = gatewayDetails.SenderId;
                _session.SaveChanges();
            }
        }
    }

    public interface IPledgeService
    {
        EventPledge CreatePledge(GroupGivingEvent @event, Account pledger);
        void ConfirmPaidPledge(GroupGivingEvent @event, EventPledge pledge, Account pledger, string transactionId);
    }
}
