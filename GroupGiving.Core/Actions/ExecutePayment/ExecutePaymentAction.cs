using System;
using System.Linq;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using Raven.Client;

namespace GroupGiving.Core.Actions.ExecutePayment
{
    public class ExecutePaymentAction
    {
        private readonly IPaymentGateway _paymentGateway;

        public ExecutePaymentAction(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public void Validate(IDocumentSession session, string eventId, string orderNumber)
        {
            // load the pledge
            var @event = session.Load<GroupGivingEvent>(eventId);
            if (@event == null)
                throw new ArgumentException("Event not found", "eventId");

            var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
            if (pledge == null)
                throw new ArgumentException("Pledge not found", "orderNumber");

            if (pledge.PaymentStatus != PaymentStatus.PaidPendingReconciliation)
                throw new InvalidOperationException("Pledge must be in PaidPendingReconciliation status to be executed");
        }

        public ExecutePaymentResponse Execute(IDocumentSession session, string eventId, string orderNumber)
        {   
            // load the pledge
            var @event = session.Load<GroupGivingEvent>(eventId);
            if (@event == null) 
                throw new ArgumentException("Event not found", "eventId");
            
            var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
            if (pledge == null)
                throw new ArgumentException("Pledge not found", "orderNumber");

            if (pledge.PaymentStatus != PaymentStatus.PaidPendingReconciliation)
                throw new InvalidOperationException("Pledge must be in PaidPendingReconciliation status to be executed");

            var request = new ExecutePaymentRequest()
                              {
                                  TransactionId = pledge.TransactionId
                              };

            // send a 'execute payment' request to paypal
            var response  = _paymentGateway
                .ExecutePayment(request);

            // if successful mark the pledge as fully paid
            if (response.Successful)
            {
                pledge.PaymentStatus = PaymentStatus.Reconciled;
            }

            pledge.PaymentGatewayHistory.Add(response.DialogueEntry);
            session.SaveChanges();

            return new ExecutePaymentResponse()
            {
                Successful = response.Successful,
                DialogueEntry = response.DialogueEntry,
                RawResponse = response.RawResponse
            };
        }
    }

    public class ExecutePaymentRequest
    {
        public string TransactionId { get; set; }
    }

    public class ExecutePaymentResponse
    {
        public static ExecutePaymentResponse Failed()
        {
            return new ExecutePaymentResponse(){Successful = false};
        }
        public static ExecutePaymentResponse Succeeded()
        {
            return new ExecutePaymentResponse() { Successful = true };
        }

        public bool Successful { get; set; }

        public Exception RawResponse { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }

        public Exception Exception { get; set; }
    }
}