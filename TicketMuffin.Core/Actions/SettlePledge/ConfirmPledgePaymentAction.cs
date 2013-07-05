using System;
using System.Collections.Generic;
using System.Linq;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using PaymentStatus = TicketMuffin.Core.Payments.PaymentStatus;

namespace TicketMuffin.Core.Actions.SettlePledge
{
    public class ConfirmPledgePaymentAction
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAccountService _accountService;
        private readonly IEmailRelayService _emailRelayService;

        public ConfirmPledgePaymentAction(IPaymentGateway paymentGateway, IAccountService accountService, IEmailRelayService emailRelayService)
        {
            _paymentGateway = paymentGateway;
            _accountService = accountService;
            this._emailRelayService = emailRelayService;
        }

        public SettlePledgeResponse ConfirmPayment(GroupGivingEvent @event, SettlePledgeRequest request)
        {
            var response = new SettlePledgeResponse();

            if (@event==null)
                throw new ArgumentException("No such event found");

            var pledge = @event.Pledges.SingleOrDefault(p => p.Payments.Any(x=>x.TransactionId == request.TransactionId));
            var payment = pledge.Payments.SingleOrDefault(x => x.TransactionId == request.TransactionId);
            if (pledge == null)
                throw new ArgumentException("No such pledge found");
            if (payment == null)
                throw new ArgumentException("No such payment found");

            if (pledge.Paid)
                throw new InvalidOperationException("Pledge is not pending payment");

            // get the payment details from payment gateway
            var paymentDetails = _paymentGateway.RetrievePaymentDetails(payment.TransactionId);
            if (pledge.PaymentGatewayHistory == null)
                pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
            pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(paymentDetails.Diagnostics.RequestContent, paymentDetails.Diagnostics.RequestContent));

            if (paymentDetails.PaymentStatus != PaymentStatus.Unauthorised) // delayed payment will be incomplete until execute payment is called
            {
                payment.PaymentStatus = PaymentStatus.AuthorisedUnsettled;
                pledge.DatePledged = 
                    DateTime.Now;
            }
            else if (paymentDetails.PaymentStatus == PaymentStatus.Settled)
            {
                payment.PaymentStatus = PaymentStatus.Settled;
                pledge.DatePledged = DateTime.Now;
            }
            if (!string.IsNullOrWhiteSpace(paymentDetails.SenderId))
            {
                pledge.PayPalEmailAddress = paymentDetails.SenderId;

                // if the pledger does not have an account then we need to create one
                var account = _accountService.RetrieveByEmailAddress(paymentDetails.SenderId);
                if (account == null)
                {
                    account = _accountService.CreateIncompleteAccount(paymentDetails.SenderId, _emailRelayService);
                    pledge.AccountId = account.Id;
                    pledge.PayPalEmailAddress = account.Email;
                }
            }
            
            return response;
        }
    }
}