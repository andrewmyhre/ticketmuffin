using System;
using System.Linq;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Actions.SettlePledge
{
    public class ConfirmPledgePaymentAction
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;

        public ConfirmPledgePaymentAction(IRepository<GroupGivingEvent> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public SettlePledgeResponse ConfirmPayment(SettlePledgeRequest request)
        {
            var response = new SettlePledgeResponse();

            var @event = _eventRepository.Retrieve(e => e.Pledges.Exists(p => p.TransactionId == request.PayPalPayKey));
            if (@event==null)
                throw new ArgumentException("No such event found");

            var pledge = @event.Pledges.Where(p => p.TransactionId == request.PayPalPayKey).SingleOrDefault();
            if (pledge == null)
                throw new ArgumentException("No such pledge found");

            if (pledge.PaymentStatus != PaymentStatus.Unpaid)
                throw new InvalidOperationException("Pledge is not pending payment");

            pledge.PaymentStatus = PaymentStatus.PaidPendingReconciliation;
            pledge.DatePledged = DateTime.Now;

            _eventRepository.SaveOrUpdate(@event);
            _eventRepository.CommitUpdates();

            return response;
        }
    }
}