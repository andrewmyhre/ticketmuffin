using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Core.Test.Integration
{
    [TestFixture]
    public class PledgeServiceTests : TicketMuffinTestsBase
    {
        private IKernel _kernel;

        [SetUp]
        public void Setup()
        {
            _kernel = new Ninject.StandardKernel(new TicketMuffin.Core.Conventions.IoCConfiguration(), new TestNinjectModule());
        }

        [Test]
        public void CanCreateAPledgeForAnEvent()
        {
            GroupGivingEvent @event = null;
            Account pledger = null;
            var documentStore = _kernel.Get<IDocumentStore>();
            using (var session = documentStore.OpenSession())
            {
                @event = CreateAnEventAndOrganiserAccount(session);
                pledger = CreateAnAccount(session);

                var pledgeService = new PledgeService(session);
                var pledge = pledgeService.CreatePledge(@event, pledger);
                Assert.That(pledge, Is.Not.Null);
            }

            

            using (var session = documentStore.OpenSession())
            {
                var actualEvent = session.Load<GroupGivingEvent>(@event.Id);
                var actualPledge = actualEvent.Pledges.SingleOrDefault(x => x.AccountId == pledger.Id);
                Assert.That(actualPledge, Is.Not.Null);
            }
        }
    }

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
                    PayPalEmail = creatorEmail,
                    PayPalFirstName = RandomString(),
                    PayPalLastName = RandomString(),
                    Email = creatorEmail
                };
            session.Store(eventCreator);


            var @event = new GroupGivingEvent()
                {
                    Title = string.Concat("Test event ", RandomString()),
                    CreatorId = eventCreator.Id
                };
            session.Store(@event);
            session.SaveChanges();

            return @event;
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
                    PayPalEmail = creatorEmail,
                    PayPalFirstName = RandomString(),
                    PayPalLastName = RandomString(),
                    Email = creatorEmail
                };
            session.Store(account);
            session.SaveChanges();

            return account;
        }
    }
}
