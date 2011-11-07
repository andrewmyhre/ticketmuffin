using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;

namespace GroupGiving.Core.Actions.CreatePledge
{
    public class MakePledgeAction
    {
        private readonly ITaxAmountResolver _tax;
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPayPalConfiguration _paypalConfiguration;

        public MakePledgeAction(ITaxAmountResolver tax, 
            IRepository<GroupGivingEvent> eventRepository, 
            IPaymentGateway paymentGateway, 
            IPayPalConfiguration paypalConfiguration)
        {
            _tax = tax;
            _eventRepository = eventRepository;
            _paymentGateway = paymentGateway;
            _paypalConfiguration = paypalConfiguration;
        }

        public CreatePledgeActionResult Attempt(GroupGivingEvent @event, Account pledgerAccount, Account organiserAccount, MakePledgeRequest request)
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
            pledge.ServiceChargeRateApplied = TicketMuffinFees.ServiceCharge;
            pledge.ServiceCharge = TicketMuffinFees.ServiceCharge*pledge.SubTotal;
            pledge.Tax = pledge.TaxRateApplied * (pledge.SubTotal + pledge.ServiceCharge);
            pledge.Total = pledge.SubTotal + pledge.ServiceChargeRateApplied + pledge.Tax;
            pledge.Attendees = (from a in request.AttendeeNames select new EventPledgeAttendee() {FullName = a}).ToList();
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
                                                                     new PaymentRecipient(_paypalConfiguration.TicketMuffinPayPalAccountEmail,
                                                                                          pledge.ServiceCharge, true),
                                                                     // TicketMuffin.com
                                                                     new PaymentRecipient(organiserAccount.PayPalEmail,
                                                                                          pledge.Total - pledge.ServiceCharge, false)
                                                                 }
                                            };

            PaymentGatewayResponse gatewayResponse = null;
            try
            {
                gatewayResponse = _paymentGateway.CreateDelayedPayment(paymentGatewayRequest);
            } catch (HttpChannelException exception)
            {
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
                pledge.TransactionId = gatewayResponse.payKey;

                pledge.Paid = false;
                pledge.PaymentStatus = PaymentStatus.Unpaid;
                @event.Pledges.Add(pledge);
                
                _eventRepository.SaveOrUpdate(@event);
                _eventRepository.CommitUpdates();
            }

            return result;
        }
    }
}