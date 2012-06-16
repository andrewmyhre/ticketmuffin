using System;
using System.Collections.Generic;
using Raven.Client;
using TicketMuffin.Core.Actions.ExecutePayment;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;

namespace TicketMuffin.Core.Actions.ActivateEvent
{
    public class ActivateEventAction
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITicketGenerator _ticketGenerator;
        private readonly IEventCultureResolver _cultureResolver;

        public ActivateEventAction(IPaymentGateway paymentGateway, ITicketGenerator ticketGenerator, IEventCultureResolver cultureResolver)
        {
            _paymentGateway = paymentGateway;
            _ticketGenerator = ticketGenerator;
            _cultureResolver = cultureResolver;
        }

        public ActivateEventResponse Execute(string eventId, IDocumentSession session)
        {
            var @event = session.Load<GroupGivingEvent>(eventId);
            if (@event == null)
            {
                throw new ArgumentException("Invalid event id", "eventId");
            }

            if (!@event.ReadyToActivate)
            {
                throw new InvalidOperationException("Event is not ready to activate");
            }

            ActivateEventResponse response = new ActivateEventResponse();
            foreach (var pledge in @event.Pledges)
            {
                if (pledge.PaymentStatus != PaymentStatus.PaidPendingReconciliation)
                {
                    continue;
                }

                var action = new ExecutePaymentAction(_paymentGateway);
                ExecutePaymentResponse executeResult = null;
                try
                {
                    executeResult = action.Execute(session, eventId, pledge.OrderNumber);
                }
                catch (Exception ex)
                {
                    executeResult = new ExecutePaymentResponse()
                    {
                        Exception = ex
                    };
                }
                response.ExecuteResults.Add(executeResult);
                if (!executeResult.Successful)
                {
                    response.Errors = true;
                }
                else
                {
                    // create a ticket for each attendee
                    foreach (var attendee in pledge.Attendees)
                    {
                        _ticketGenerator.CreateTicket(@event, pledge, attendee, _cultureResolver.ResolveCulture(@event));
                    }
                }
            }

            @event.State = EventState.Activated;
            session.SaveChanges();

            return response;
        }

        public class ActivateEventResponse
        {
            public ActivateEventResponse()
            {
                ExecuteResults = new List<ExecutePaymentResponse>();
            }

            public List<ExecutePaymentResponse> ExecuteResults { get; set; }

            public bool Errors { get; set; }
        }
    }
}
