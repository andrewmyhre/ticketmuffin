﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Configuration;
using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Core.Actions.CreatePledge
{
    public class MakePledgeAction
    {
        private readonly ITaxAmountResolver _tax;
        private readonly IPaymentGateway _paymentGateway;
        private readonly AdaptiveAccountsConfiguration _paypalConfiguration;
        private readonly IDocumentSession _documentSession;

        public MakePledgeAction(ITaxAmountResolver tax, 
            IPaymentGateway paymentGateway, 
            AdaptiveAccountsConfiguration paypalConfiguration,
            IDocumentSession documentSession)
        {
            _tax = tax;
            _paymentGateway = paymentGateway;
            _paypalConfiguration = paypalConfiguration;
            _documentSession = documentSession;
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
            pledge.OrderNumber = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");

            // calculate split

            // make request to payment gateway
            var paymentGatewayRequest = new PaymentGatewayRequest()
                                            {
                                                Amount = pledge.Total,
                                                OrderMemo = "Tickets for " + @event.Title,
                                                SuccessCallbackUrl = _paypalConfiguration.SuccessCallbackUrl,
                                                FailureCallbackUrl = _paypalConfiguration.FailureCallbackUrl,
                                                Recipients = new List<PaymentRecipient>()
                                                                    {
                                                                        // TicketMuffin.com
                                                                        new PaymentRecipient(
                                                                            _paypalConfiguration.PayPalAccountEmail,
                                                                            pledge.Total, true),
                                                                    // event organiser
                                                                        new PaymentRecipient(
                                                                            organiserAccount.PayPalEmail,
                                                                            pledge.Total - pledge.ServiceCharge, false)
                                                                    },
                                                                    CurrencyCode = Enum.GetName(typeof(Currency), @event.Currency)
                                            };

            PaymentGatewayResponse gatewayResponse = null;
            try
            {
                gatewayResponse = _paymentGateway.CreateDelayedPayment(paymentGatewayRequest);
                if (pledge.PaymentGatewayHistory==null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(gatewayResponse.DialogueEntry);
            }
            catch (HttpChannelException exception)
            {
                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(exception.FaultMessage.Raw);
                @event.Pledges.Add(pledge);
                _documentSession.SaveChanges();
                return new CreatePledgeActionResult()
                            {
                                Succeeded = false,
                                Exception = exception,
                                GatewayResponse = new PaymentGatewayResponse()
                                                        {
                                                            Error = new ResponseError()
                                                                        {
                                                                            Message = exception.Message
                                                                        }
                                                        }
                            };
            }

            result.GatewayResponse = gatewayResponse;
            if (gatewayResponse.PaymentExecStatus == "CREATED")
            {
                result.Succeeded = true;
                result.TransactionId = pledge.TransactionId = gatewayResponse.payKey;

                pledge.Paid = false;
                pledge.PaymentStatus = PaymentStatus.Unpaid;
                @event.Pledges.Add(pledge);

                _documentSession.SaveChanges();
            }

            return result;
        }
    }
}