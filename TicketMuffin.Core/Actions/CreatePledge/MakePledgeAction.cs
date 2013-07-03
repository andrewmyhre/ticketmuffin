using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Actions.CreatePledge
{
    public class MakePledgeAction
    {
        private readonly ITaxAmountResolver _tax;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IDocumentSession _documentSession;
        private readonly IOrderNumberGenerator _orderNumberGenerator;

        public MakePledgeAction(ITaxAmountResolver tax, 
            IPaymentGateway paymentGateway, 
            IDocumentSession documentSession,
            IOrderNumberGenerator orderNumberGenerator)
        {
            _tax = tax;
            _paymentGateway = paymentGateway;
            _documentSession = documentSession;
            _orderNumberGenerator = orderNumberGenerator;
        }

        public CreatePledgeActionResult Attempt(string eventId, Account organiserAccount, MakePledgeRequest request)
        {
            var result = new CreatePledgeActionResult();
            var pledge = new EventPledge();

            var @event = _documentSession.Load<GroupGivingEvent>(eventId);

            bool eventWasOn = @event.IsOn;

            if (@event.IsFull)
            {
                throw new InvalidOperationException("This event is full");
            }

            if (@event.SalesEnded)
            {
                throw new InvalidOperationException("Sales for this event have ended");
            }

            if (@event.IsOn)
            {
                int spacesLeft = (@event.MaximumParticipants ?? 0) - @event.PaidAttendeeCount;
                if (request.AttendeeNames.Count() > spacesLeft)
                {
                    throw new InvalidOperationException(
                        string.Format("There are only {0} spaces left for this event", spacesLeft));
                }
            }

            // calculate sub total to charge
            pledge.SubTotal = @event.TicketPrice*request.AttendeeNames.Count();

            // apply tax
            // todo: tax being ignored
            //pledge.TaxRateApplied = _tax.LookupTax(@event.Country);
            pledge.TaxRateApplied = 0;
            pledge.ServiceChargeRateApplied = TicketMuffinFees.ServiceCharge;
            pledge.ServiceCharge = TicketMuffinFees.ServiceCharge*pledge.SubTotal;
            pledge.Tax = pledge.TaxRateApplied*(pledge.SubTotal + pledge.ServiceCharge);
            pledge.Total = pledge.SubTotal;
            pledge.Attendees =
                (from a in request.AttendeeNames select new EventPledgeAttendee() {FullName = a}).ToList();
            pledge.AccountEmailAddress = request.PayPalEmailAddress;
            pledge.OrderNumber = _orderNumberGenerator.Generate(@event);

            // calculate split

            // make request to payment gateway
            
            IPaymentAuthoriseResponse gatewayResponse = null;
            try
            {
                var paymentMemo = "Tickets for " + @event.Title;
                var currencyCode = Enum.GetName(typeof (Currency), @event.Currency);
                gatewayResponse = _paymentGateway.AuthoriseCharge(pledge.Total, currencyCode, paymentMemo, organiserAccount.PayPalEmail);
                if (pledge.PaymentGatewayHistory==null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(gatewayResponse.Diagnostics.RequestContent, gatewayResponse.Diagnostics.ResponseContent));
            }
            catch (Exception exception)
            {
                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(exception));
                @event.Pledges.Add(pledge);
                _documentSession.SaveChanges();
                return new CreatePledgeActionResult()
                            {
                                Succeeded = false,
                                Exception = exception,
                            };
            }

            result.Succeeded = true;
            if (gatewayResponse.Status == PaymentStatus.Unsettled)
            {
                result.Succeeded = true;
                result.TransactionId = pledge.TransactionId = gatewayResponse.TransactionId;

                pledge.Paid = false;
                pledge.PaymentStatus = PaymentStatus.Unpaid;
                @event.Pledges.Add(pledge);

                _documentSession.SaveChanges();
            }

            return result;
        }
    }
}