using System;
using System.Collections.Generic;
using System.Linq;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Model;

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

            var pledge = @event.Pledges.SingleOrDefault(p => p.TransactionId == request.PayPalPayKey);
            if (pledge == null)
                throw new ArgumentException("No such pledge found");

            if (pledge.PaymentStatus != PaymentStatus.Unpaid)
                throw new InvalidOperationException("Pledge is not pending payment");

            // get the payment details from payment gateway
            var paymentDetails =
                _paymentGateway.RetrievePaymentDetails(pledge.TransactionId);
            if (pledge.PaymentGatewayHistory == null)
                pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
            pledge.PaymentGatewayHistory.Add(new DialogueHistoryEntry(paymentDetails.Raw.Request, paymentDetails.Raw.Response));

            if (paymentDetails.status == "INCOMPLETE") // delayed payment will be incomplete until execute payment is called
            {
                pledge.PaymentStatus = PaymentStatus.PaidPendingReconciliation;
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;
            }
            else if (paymentDetails.status == "CREATED")
            {
                pledge.PaymentStatus = PaymentStatus.Reconciled;
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;
            }
            if (!string.IsNullOrWhiteSpace(paymentDetails.senderEmail))
            {
                pledge.AccountEmailAddress = paymentDetails.senderEmail;

                // if the pledger does not have an account then we need to create one
                var account = _accountService.RetrieveByEmailAddress(paymentDetails.senderEmail);
                if (account == null)
                {
                    account = _accountService.CreateIncompleteAccount(paymentDetails.senderEmail, _emailRelayService);
                    pledge.AccountId = account.Id;
                    pledge.AccountEmailAddress = account.Email;
                }
            }
            
            return response;
        }
    }
}