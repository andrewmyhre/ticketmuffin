using System;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
using log4net;

namespace TicketMuffin.Core.Actions.ExecutePayment
{
    public class ExecutePaymentAction
    {
        private ILog _logger = log4net.LogManager.GetLogger(typeof (ExecutePaymentAction));
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

            if (pledge.PaymentStatus != PaymentStatus.Unsettled)
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

            if (pledge.PaymentStatus != PaymentStatus.Unsettled)
                throw new InvalidOperationException("Pledge must be in PaidPendingReconciliation status to be executed");

            // send a 'execute payment' request to paypal
            try
            {
                var response = _paymentGateway.CapturePayment(pledge.TransactionId);

                // if successful mark the pledge as fully paid
                pledge.PaymentStatus = PaymentStatus.Settled;

                var dialogueHistoryEntry = new DialogueHistoryEntry(response.Diagnostics.RequestContent, response.Diagnostics.ResponseContent);
                pledge.PaymentGatewayHistory.Add(dialogueHistoryEntry);
                session.SaveChanges();
                return new ExecutePaymentResponse{DialogueEntry = dialogueHistoryEntry};

            } catch (Exception fault)
            {
                _logger.Error("Payment gateway error", fault);

                throw;
            }
        }
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