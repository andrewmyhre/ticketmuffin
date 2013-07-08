﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
        private readonly IIdentity _userIdentity;
        private readonly IAccountService _accountService;
        private readonly ICurrencyStore _currencyStore;

        public MakePledgeAction(ITaxAmountResolver tax, 
            IPaymentGateway paymentGateway, 
            IDocumentSession documentSession,
            IOrderNumberGenerator orderNumberGenerator,
            IIdentity userIdentity,
            IAccountService accountService,
            ICurrencyStore currencyStore)
        {
            _tax = tax;
            _paymentGateway = paymentGateway;
            _documentSession = documentSession;
            _orderNumberGenerator = orderNumberGenerator;
            _userIdentity = userIdentity;
            _accountService = accountService;
            _currencyStore = currencyStore;
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
            pledge.PayPalEmailAddress = request.PayPalEmailAddress;
            
            if (_userIdentity.IsAuthenticated)
            {
                var account = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
                if (account == null)
                    throw new InvalidOperationException("The account record for the logged in user could not be found");
                pledge.AccountId = account.Id;
                pledge.AccountEmailAddress = account.Email;
                pledge.AccountName = string.Join(" ", account.FirstName, account.LastName);
            }
            else
            {
                pledge.AccountEmailAddress = request.PayPalEmailAddress;
            }
            pledge.OrderNumber = _orderNumberGenerator.Generate(@event);

            // calculate split

            // make request to payment gateway
            
            PaymentAuthoriseResponse gatewayResponse = null;
            try
            {
                var paymentMemo = "Tickets for " + @event.Title;
                var currency = _currencyStore.GetCurrencyByIso4217Code(@event.CurrencyNumericCode);
                gatewayResponse = _paymentGateway
                    .AuthoriseCharge(pledge.Total, currency.Iso4217AlphaCode, paymentMemo, organiserAccount.PaymentGatewayId);
                
                if (gatewayResponse==null)
                    throw new Exception("Payment gateway did not return a response");

                if (pledge.PaymentGatewayHistory==null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(gatewayResponse.Diagnostics.RequestContent, gatewayResponse.Diagnostics.ResponseContent));

                if (gatewayResponse.Successful)
                {
                    // create a new payment
                    var payment = new Payment();
                    payment.TransactionId = gatewayResponse.TransactionId;
                    payment.PaymentStatus = gatewayResponse.Status;
                    pledge.Payments.Add(payment);

                    result.Succeeded = true;
                    result.TransactionId = gatewayResponse.TransactionId;
                    result.PaymentPageUrl = gatewayResponse.RedirectUrl;
                    @event.Pledges.Add(pledge);
                    _documentSession.SaveChanges();
                }

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

            if (gatewayResponse.Status == PaymentStatus.Unauthorised)
            {
                result.AuthorisationRequired = true;
            }
            result.OrderNumber = pledge.OrderNumber;

            return result;
        }
    }
}