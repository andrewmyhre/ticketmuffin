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

            if (pledge.Paid)
                throw new InvalidOperationException("Pledge has already been paid for");
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

            var payment = pledge.Payments.SingleOrDefault(x => x.PaymentStatus == PaymentStatus.AuthorisedUnsettled);

            if (payment == null)
                throw new InvalidOperationException("Payment to be executed does not exist");

            // send a 'execute payment' request to paypal
            try
            {
                var response = _paymentGateway.CapturePayment(payment.TransactionId);

                // if successful mark the pledge as fully paid
                payment.PaymentStatus = PaymentStatus.Settled;

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