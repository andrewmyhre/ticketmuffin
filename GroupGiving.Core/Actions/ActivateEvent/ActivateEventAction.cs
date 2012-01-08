﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Actions.ExecutePayment;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using Raven.Client;

namespace GroupGiving.Core.Actions.ActivateEvent
{
    public class ActivateEventAction
    {
        private readonly IDocumentStore _documentStore;
        private readonly IPaymentGateway _paymentGateway;

        public ActivateEventAction(IDocumentStore documentStore, IPaymentGateway paymentGateway)
        {
            _documentStore = documentStore;
            _paymentGateway = paymentGateway;
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

            }

            @event.State = EventState.Activated;
            session.SaveChanges();

            return response;
        }

        public ActivateEventResponse Execute(string eventId)
        {
            using (var session = _documentStore.OpenSession())
            {
                return Execute(eventId, session);
            }
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
