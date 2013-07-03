using System.Collections.Generic;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Services
{
    public interface IPaymentService
    {
        string GetPaymentUrlForPledge(GroupGivingEvent @event, Account eventOrganiser, EventPledge pledge);
        PaymentDetailsResponse GetPaymentDetails(Payment payment);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IDocumentSession _session;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentService(IDocumentSession session, IPaymentGateway paymentGateway)
        {
            _session = session;
            _paymentGateway = paymentGateway;
        }

        public string GetPaymentUrlForPledge(GroupGivingEvent @event, Account eventOrganiser, EventPledge pledge)
        {
            // determine charges
            pledge.SubTotal = @event.TicketPrice * pledge.Attendees.Count;
            pledge.ServiceCharge = pledge.SubTotal*1.05m;
            pledge.Total = pledge.SubTotal + pledge.ServiceCharge;
            _session.SaveChanges();

            string memo = "Tickets for " + @event.Title;
            string currencyCode = @event.Iso4217Alpha3Code;
            string successUrl = "";
            string failureUrl = "";
            var receivers = new[]
                {
                    // primary (TicketMuffin)
                    new Receiver {EmailAddress = eventOrganiser.Email, Amount = pledge.ServiceCharge, Primary = true},

                    // secondary (event organiser)
                    new Receiver{EmailAddress=eventOrganiser.PaymentGatewayId, Amount=pledge.SubTotal, Primary=false}
                };

            var response = _paymentGateway.CreatePayment(memo, currencyCode, successUrl, failureUrl, receivers);

            // create a payment record
            pledge.Payments.Add(new Payment() { TransactionId = response.TransactionId, PaymentGatewayName = _paymentGateway.Name, PaymentStatus = PaymentStatus.Unpaid });
            _session.SaveChanges();

            return response.PaymentUrl;
        }

        public PaymentDetailsResponse GetPaymentDetails(Payment payment)
        {
            var response = _paymentGateway.RetrievePaymentDetails(payment.TransactionId);
            return response;
        }
    }
}