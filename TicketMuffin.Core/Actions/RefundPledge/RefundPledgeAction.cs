using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Actions.RefundPledge
{
    public class RefundPledgeAction
    {
        private IPaymentGateway _paymentGateway;

        public RefundPledgeAction(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public IPaymentRefundResponse Execute(IDocumentSession session, string eventId, string pledgeOrderNumber)
        {
                var @event = session.Load<GroupGivingEvent>(eventId);
                if (@event == null)
                {
                    throw new ArgumentException("No event could be found matching that id");
                }


                var pledge = @event.Pledges.Where(p => p.OrderNumber == pledgeOrderNumber).FirstOrDefault();

                if (pledge == null)
                {
                    throw new ArgumentException("No pledge could be found matching that order number");
                }

                IPaymentRefundResponse refundResponse = null;
                try
                {
                    refundResponse = _paymentGateway.Refund(pledge.TransactionId, pledge.Total, pledge.AccountEmailAddress);
                }
                catch (Exception exception)
                {
                    if (pledge.PaymentGatewayHistory == null)
                        pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                    pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(exception));
                    session.SaveChanges();
                    throw;
                }

                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(refundResponse.Diagnostics.RequestContent, refundResponse.Diagnostics.ResponseContent));

                if (refundResponse.Successful)
                {
                    pledge.DateRefunded = DateTime.Now;
                    pledge.PaymentStatus = PaymentStatus.Refunded;
                    pledge.Paid = false;
                }

                session.SaveChanges();

                return refundResponse;
        }
    }
}
