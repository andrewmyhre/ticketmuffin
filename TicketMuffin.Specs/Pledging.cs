using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using TechTalk.SpecFlow;
using TicketMuffin.Core.Actions.CreatePledge;
using TicketMuffin.Core.Actions.SettlePledge;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Configuration;

namespace TicketMuffin.Specs
{
    [Binding]
    public class Pledging
    {
        private GroupGivingEvent _event;
        private Mock<ITaxAmountResolver> taxResolver=new Mock<ITaxAmountResolver>();
        private Mock<IPaymentGateway> paymentGateway =new Mock<IPaymentGateway>();
        private Mock<AdaptiveAccountsConfiguration> paypalConfiguration=new Mock<AdaptiveAccountsConfiguration>();
        private IDocumentStore documentStore;
        private IDocumentSession documentSession;
        private Account OrganiserAccount;
        private CreatePledgeActionResult pledgeResult;
        private Exception _exception;
        private Mock<IAccountService> accountService=new Mock<IAccountService>();
        private Mock<IEmailRelayService> emailRelayService=new Mock<IEmailRelayService>();

        [BeforeScenario]
        public void SetUp()
        {
            Helpers.NoTax(taxResolver);
            Helpers.CreateDelayedPaymentSucceeds(paymentGateway);
            Helpers.PaymentGatewayReturnsSuccessful(paymentGateway);

            documentStore = new EmbeddableDocumentStore()
            {
                RunInMemory = true
            };
            documentStore.Initialize();
            documentSession = documentStore.OpenSession();
            OrganiserAccount = Helpers.TestAccount();
            documentSession.Store(OrganiserAccount);
            accountService.Setup(x => x.RetrieveByEmailAddress(OrganiserAccount.Email)).Returns(OrganiserAccount);

            _event = new GroupGivingEvent()
            {
                SalesEndDateTime = DateTime.Now.AddDays(10),
                MinimumParticipants = 10,
                MaximumParticipants = 20,
                OrganiserId = OrganiserAccount.Id,
                State = EventState.SalesReady
            };

            documentSession.Store(_event);

            documentSession.SaveChanges();
        }

        [AfterScenario]
        public void TearDown()
        {
            documentSession.Dispose();
            documentStore.Dispose();
        }

        [Given(@"Sales have ended")]
        public void GivenSalesHaveEnded()
        {
            _event.SalesEndDateTime = DateTime.Now.AddDays(-1);

            documentSession.SaveChanges();

            Assert.That(_event.SalesEnded, Is.True);
        }

        [When(@"I pledge to attend")]
        public void WhenIPledgeToAttend()
        {
            var createPledge = 
                new MakePledgeAction(taxResolver.Object, paymentGateway.Object, documentSession, new OrderNumberGenerator());

            MakePledgeRequest pledgeRequest =new MakePledgeRequest()
            {
                AttendeeNames = new string[]{"tom","dick","harry"}
            };
            
            try
            {
                pledgeResult = createPledge.Attempt(_event.Id, OrganiserAccount, pledgeRequest);
            } catch (Exception exception)
            {
                _exception = exception;
            }
        }


        [Then(@"the pledge should not be accepted with message ""(.*)""")]
        public void ThenThePledgeShouldNotBeAccepted(string message)
        {
            Assert.That(_exception, Is.Not.Null);
            Assert.That(_exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(_exception.Message, Is.StringMatching(message));
        }

        [Given(@"The event is full")]
        public void GivenTheEventIsFull()
        {
            _event.MaximumParticipants = 10;
            _event.MinimumParticipants = 5;
            _event.Pledges
                = new List<EventPledge>()
                      {
                          new EventPledge()
                              {
                                  Attendees =
                                      new List<EventPledgeAttendee>(Helpers.Attendees(10)),
                                      Payments = new List<Payment>(new[]{new Payment(){PaymentStatus=PaymentStatus.Settled,TransactionId = "98765"}, }),
                                      OrderNumber=Guid.NewGuid().ToString(),
                              }
                      };

            documentSession.SaveChanges();
        }

        [Given(@"Sales have not ended yet")]
        public void GivenSalesHaveNotEndedYet()
        {
            _event.SalesEndDateTime = DateTime.Now.AddDays(10);

            documentSession.SaveChanges();
        }

        [Given(@"The event needs one more pledge to be ready to activate")]
        public void GivenTheEventNeedsOneMorePledgeToBeReadyToActivate()
        {
            _event.Pledges
                = new List<EventPledge>()
                      {
                          new EventPledge()
                              {
                                  Attendees =
                                      new List<EventPledgeAttendee>(Helpers.Attendees(9)),
                                      Payments = new List<Payment>(new[]{new Payment(){PaymentStatus = PaymentStatus.Unsettled,TransactionId="12345"} }),
                                      DatePledged = DateTime.Now,
                                      OrderNumber = Guid.NewGuid().ToString(),
                              }
                      };
            documentSession.SaveChanges();
            Assert.That(_event.PaidAttendeeCount, Is.EqualTo(_event.MinimumParticipants-1));
        }

        [When(@"I complete the payment through paypal")]
        public void WhenICompleteThePaymentThroughPaypal()
        {
            var confirmPledge
                =new ConfirmPledgePaymentAction(paymentGateway.Object, accountService.Object, emailRelayService.Object);

            var request=new SettlePledgeRequest()
                                            {
                                                TransactionId=pledgeResult.TransactionId
                                            };
            confirmPledge.ConfirmPayment(_event, request);

            // check that the pledge took
            var pledge = _event.Pledges.SingleOrDefault(p => p.Payments.Any(pmt=>pmt.TransactionId==pledgeResult.TransactionId));
            Assert.That(pledge, Is.Not.Null);
            Assert.That(pledge.Paid, Is.True);
            var payment = pledge.Payments.SingleOrDefault(p => p.TransactionId == pledgeResult.TransactionId);
            Assert.That(payment.PaymentStatus, Is.EqualTo(PaymentStatus.Settled));
        }


        [Then(@"the event should be ready to activate")]
        public void ThenTheEventShouldBeReadyToActivate()
        {
            Assert.That(_event.ReadyToActivate, Is.True);
        }

        [Given(@"The event is not full")]
        public void GivenTheEventIsNotFull()
        {
            _event.MaximumParticipants = _event.AttendeeCount + 10;
            documentSession.SaveChanges();
        }

        [Then(@"the pledge should be accepted")]
        public void ThenThePledgeShouldBeAccepted()
        {
            Assert.That(pledgeResult.Succeeded, Is.True);
        }

        [Given(@"The event has 2 spaces left")]
        public void GivenTheEventHas2SpacesLeft()
        {
            _event.MinimumParticipants = 5;
            _event.MaximumParticipants = 10;
            _event.Pledges=new List<EventPledge>()
            {
                new EventPledge(){
                    Attendees = Helpers.Attendees(8).ToList(), 
                    Payments = new List<Payment>(new[]{new Payment(){PaymentStatus = PaymentStatus.Settled}, })
                },
            };
            documentSession.SaveChanges();
        }

    }
}
