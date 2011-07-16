using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;

namespace GroupGiving.Core.Actions.CreatePledge
{
    public class MakePledgeAction
    {
        private readonly ITaxAmountResolver _tax;
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPayPalConfiguration _paypalConfiguration;
        private readonly IEmailCreationService _emailCreationService;
        private readonly IEmailRelayService _emailRelayService;

        public MakePledgeAction(ITaxAmountResolver tax, 
            IRepository<GroupGivingEvent> eventRepository, 
            IPaymentGateway paymentGateway, 
            IPayPalConfiguration paypalConfiguration, 
            IEmailCreationService emailCreationService,
            IEmailRelayService emailRelayService)
        {
            _tax = tax;
            _eventRepository = eventRepository;
            _paymentGateway = paymentGateway;
            _paypalConfiguration = paypalConfiguration;
            _emailCreationService = emailCreationService;
            _emailRelayService = emailRelayService;
        }

        public CreatePledgeActionResult Attempt(GroupGivingEvent @event, Account account, MakePledgeRequest request)
        {
            var result = new CreatePledgeActionResult();
            var pledge = new EventPledge();
            bool eventWasOn = @event.IsOn;

            if (@event.IsFull)
            {
                throw new InvalidOperationException("Maximum attendees exceeded");
            }
 
            if (@event.IsOn)
            {
                int spacesLeft = (@event.MaximumParticipants??0) - @event.PledgeCount;
                if (request.AttendeeNames.Count() > spacesLeft)
                {
                    throw new InvalidOperationException(string.Format("There are only {0} spaces left for this event", spacesLeft));
                }
            }

            // calculate sub total to charge
            pledge.SubTotal = @event.TicketPrice * request.AttendeeNames.Count();

            // apply tax
            pledge.TaxRateApplied = _tax.LookupTax(@event.Country);
            pledge.Tax = pledge.TaxRateApplied * pledge.SubTotal;
            pledge.Total = pledge.SubTotal + pledge.Tax;
            pledge.Attendees = (from a in request.AttendeeNames select new EventPledgeAttendee() {FullName = a}).ToList();
            pledge.EmailAddress = request.PayPalEmailAddress;
            pledge.OrderNumber = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");

            // make request to payment gateway
            var gatewayResponse = _paymentGateway.MakeRequest(
                new PaymentGatewayRequest()
                    {
                        Amount = pledge.Total,
                        OrderMemo = "Tickets for " + @event.Title,
                        SuccessCallbackUrl = _paypalConfiguration.SuccessCallbackUrl,
                        FailureCallbackUrl = _paypalConfiguration.FailureCallbackUrl
                    });

            result.GatewayResponse = gatewayResponse;
            if (result.GatewayResponse.Errors == null || result.GatewayResponse.Errors.Count() == 0)
            {
                result.Succeeded = true;
                pledge.TransactionId = gatewayResponse.TransactionId;

                @event.Pledges.Add(pledge);
                _eventRepository.SaveOrUpdate(@event);
                _eventRepository.CommitUpdates();
            }

            return result;
        }
    }
}