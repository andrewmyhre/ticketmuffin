using System;
using System.Text;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Test.Integration
{
    public class TicketMuffinTestsBase
    {
        private static Random _random = new Random();

        protected static string RandomString(int length=10)
        {
            var sb = new StringBuilder();
            while (length-- > 0)
            {
                sb.Append((char) _random.Next((int) 'A', (int) 'Z'));
            }
            return sb.ToString();
        }

        protected static GroupGivingEvent CreateAnEventAndOrganiserAccount(IDocumentSession session)
        {
            var creatorEmail = string.Concat(RandomString(), "@integrationtest.com");
            var eventCreator = new Account()
                {
                    FirstName = RandomString(),
                    LastName = RandomString(),
                    AccountType = AccountType.Individual,
                    Culture = "en-GB",
                    PaymentGatewayId = creatorEmail,
                    PayPalFirstName = RandomString(),
                    PayPalLastName = RandomString(),
                    Email = creatorEmail
                };
            session.Store(eventCreator);


            var @event = new GroupGivingEvent()
                {
                    Title = string.Concat("Test event ", RandomString()),
                    OrganiserId = eventCreator.Id
                };
            session.Store(@event);
            session.SaveChanges();

            return @event;
        }

        protected static EventPledge AddAPledgeToEvent(GroupGivingEvent @event, Account pledger)
        {
            var pledge = new EventPledge()
            {
                AccountEmailAddress = pledger.Email,
                AccountId = pledger.Id,
                AccountName = pledger.FirstName,
                DatePledged = DateTime.Now,
                Total = 10m
            };
            @event.Pledges.Add(pledge);
            return pledge;
        }

        protected Account CreateAnAccount(IDocumentSession session)
        {
            var creatorEmail = string.Concat(RandomString(), "@integrationtest.com");
            var account = new Account()
                {
                    FirstName = RandomString(),
                    LastName = RandomString(),
                    AccountType = AccountType.Individual,
                    Culture = "en-GB",
                    PaymentGatewayId = creatorEmail,
                    PayPalFirstName = RandomString(),
                    PayPalLastName = RandomString(),
                    Email = creatorEmail
                };
            session.Store(account);
            session.SaveChanges();

            return account;
        }

        protected Payment AddAPaymentToPledge(EventPledge pledge, PaymentStatus paymentStatus, string transactionId)
        {
            var payment = new Payment() {PaymentStatus = paymentStatus, TransactionId = transactionId};
            pledge.Payments.Add(payment);
            return payment;
        }
    }
}