using System;
using System.Linq;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using Raven.Client;

namespace GroupGiving.Core.Actions.SettlePledge
{
    public class ConfirmPledgePaymentAction
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAccountService _accountService;
        private readonly IEmailRelayService _emailRelayService;

        public ConfirmPledgePaymentAction(IRepository<GroupGivingEvent> eventRepository, 
            IPaymentGateway paymentGateway, IAccountService accountService, IEmailRelayService emailRelayService)
        {
            _eventRepository = eventRepository;
            _paymentGateway = paymentGateway;
            _accountService = accountService;
            this._emailRelayService = emailRelayService;
        }

        public SettlePledgeResponse ConfirmPayment(SettlePledgeRequest request)
        {
            var response = new SettlePledgeResponse();

            var @event = _eventRepository.Retrieve(e => e.Pledges.Exists(p => p.TransactionId == request.PayPalPayKey));
            if (@event==null)
                throw new ArgumentException("No such event found");

            var pledge = @event.Pledges.Where(p => p.TransactionId == request.PayPalPayKey).SingleOrDefault();
            if (pledge == null)
                throw new ArgumentException("No such pledge found");

            if (pledge.PaymentStatus != PaymentStatus.Unpaid)
                throw new InvalidOperationException("Pledge is not pending payment");

            // get the payment details from payment gateway
            var paymentDetails =
                _paymentGateway.RetrievePaymentDetails(new PaymentDetailsRequest()
                                                           {TransactionId = pledge.TransactionId});

            if (paymentDetails.Status == "INCOMPLETE") // delayed payment will be incomplete until execute payment is called
            {
                pledge.PaymentStatus = PaymentStatus.PaidPendingReconciliation;
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;
            }
            else if (paymentDetails.Status == "CREATED")
            {
                pledge.PaymentStatus = PaymentStatus.Reconciled;
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;
            }
            if (!string.IsNullOrWhiteSpace(paymentDetails.SenderEmailAddress))
            {
                pledge.AccountEmailAddress = paymentDetails.SenderEmailAddress;

                // if the pledger does not have an account then we need to create one
                var account = _accountService.RetrieveByEmailAddress(paymentDetails.SenderEmailAddress);
                    if (account == null)
                    {
                        account = _accountService.CreateIncompleteAccount(paymentDetails.SenderEmailAddress, _emailRelayService);
                        pledge.AccountId = account.Id;
                        pledge.AccountEmailAddress = account.Email;
                    }
            }
            

            _eventRepository.SaveOrUpdate(@event);
            _eventRepository.CommitUpdates();

            return response;
        }
    }
}