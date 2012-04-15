using System;
using System.Linq;
using GroupGiving.Core.Actions.RefundPledge;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using Raven.Client;
using RefundResponse = GroupGiving.PayPal.Model.RefundResponse;

namespace GroupGiving.Core.Actions.CancelEvent
{
    public class CancelEventAction
    {
        private readonly IPaymentGateway _paymentGateway;

        public CancelEventAction(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public CancelEventResponse Execute(IDocumentSession session, string eventId)
        {
            var @event = session.Load<GroupGivingEvent>(eventId);
            return Execute(session, @event);
        }

        public CancelEventResponse Execute(IDocumentSession session, int eventId)
        {
            var @event = session.Load<GroupGivingEvent>(eventId);
            return Execute(session, @event);
        }

        private CancelEventResponse Execute(IDocumentSession session, GroupGivingEvent @event)
        {
            if (@event.State == EventState.Cancelled || @event.State == EventState.Completed)
            {
                throw new InvalidOperationException("Event is completed or already cancelled");
            }

            var action = new RefundPledgeAction(_paymentGateway);
            var pledges =
                @event.Pledges.Where(
                    p =>
                    p.PaymentStatus == PaymentStatus.Reconciled ||
                    p.PaymentStatus == PaymentStatus.PaidPendingReconciliation);

            bool noProblems = true;
            foreach (var pledge in pledges)
            {
                RefundResponse refundResponse = null;
                try
                {
                    refundResponse = action.Execute(session, @event.Id, pledge.OrderNumber);
                    if (!refundResponse.Successful)
                    {
                        noProblems = false;
                        break;
                    }
                }
                catch (Exception exception)
                {
                    noProblems = false;
                    break;
                }
            }

            if (noProblems)
            {
                @event.State = EventState.Cancelled;
                session.SaveChanges();

                return CancelEventResponse.Successful;
            }

            return CancelEventResponse.Failed;
            
        }

    }
}