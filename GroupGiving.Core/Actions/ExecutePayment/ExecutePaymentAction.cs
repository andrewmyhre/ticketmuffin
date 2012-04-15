using System;
using System.Linq;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using Raven.Client;
using log4net;

namespace GroupGiving.Core.Actions.ExecutePayment
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
                                  PayKey = pledge.TransactionId
                              };

            // send a 'execute payment' request to paypal
            try
            {
                var response = _paymentGateway.ExecutePayment(request);

                // if successful mark the pledge as fully paid
                pledge.PaymentStatus = PaymentStatus.Reconciled;

                pledge.PaymentGatewayHistory.Add(response.Raw);
                session.SaveChanges();
                return new ExecutePaymentResponse()
                           {
                               DialogueEntry = new DialogueHistoryEntry(response.Raw.Request, response.Raw.Response),
                           };
            } catch (HttpChannelException fault)
            {
                _logger.Warn("paypal error", fault);
                _logger.Warn(fault + "\r\n" + fault.Message + "\r\n" + fault.FaultMessage.Raw.Request + "\r\n" + fault.FaultMessage.Raw.Response);

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