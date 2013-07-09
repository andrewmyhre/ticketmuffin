using System;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IDocumentSession _session;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ICurrencyStore _currencyStore;

        public PaymentService(IDocumentSession session, IPaymentGateway paymentGateway, ICurrencyStore currencyStore)
        {
            _session = session;
            _paymentGateway = paymentGateway;
            _currencyStore = currencyStore;
        }

        public string GetPaymentUrlForPledge(GroupGivingEvent @event, Account eventOrganiser, EventPledge pledge)
        {
            // determine charges
            pledge.SubTotal = @event.TicketPrice * pledge.Attendees.Count;
            pledge.ServiceCharge = pledge.SubTotal*1.05m;
            pledge.Total = pledge.SubTotal + pledge.ServiceCharge;
            _session.SaveChanges();

            var allCurrencies = _currencyStore.AllCurrencies();
            var currency = _currencyStore.GetCurrencyByIso4217Code(@event.CurrencyNumericCode);
            if (currency == null)
            {
                throw new Exception(string.Format("Could not find a currency with Iso code '{0}'", @event.CurrencyNumericCode));
            }
            string memo = "Tickets for " + @event.Title;
            string successUrl = "";
            string failureUrl = "";
            var receivers = new[]
                {
                    // primary (TicketMuffin)
                    new Receiver {EmailAddress = eventOrganiser.Email, Amount = pledge.ServiceCharge, Primary = true},

                    // secondary (event organiser)
                    new Receiver{EmailAddress=eventOrganiser.PaymentGatewayId, Amount=pledge.SubTotal, Primary=false}
                };

            var response = _paymentGateway.CreatePayment(memo, currency.Iso4217AlphaCode, successUrl, failureUrl, receivers);

            // create a payment record
            pledge.Payments.Add(new Payment()
                {
                    TransactionId = response.TransactionId, 
                    PaymentGatewayName = _paymentGateway.Name, 
                    PaymentStatus = PaymentStatus.Created,
                    Total = pledge.Total,
                    SettlementCurrencyCode = currency.Iso4217AlphaCode
                });
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