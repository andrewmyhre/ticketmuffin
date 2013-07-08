using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using TicketMuffin.Core;
using TicketMuffin.Core.Actions.CreatePledge;
using TicketMuffin.Core.Actions.SettlePledge;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Model;

namespace GroupGiving.Test.Unit.Pledging
{
    [TestFixture]
    public class MakingAPledgeWithValidRequest : PledgeTestsBase
    {
        protected decimal ExpectedTotalCharge;
        protected decimal ExpectedTaxValue;
        protected string[] Attendee;
        protected decimal TaxRate;
        private CreatePledgeActionResult _result;
        private readonly Mock<IIdentity> _userIdentity = new Mock<IIdentity>();

        [SetUp]
        public virtual void SetUp()
        {
            TaxRate = 0.15m;
            Attendee = new string[] {"some guy"};
            ExpectedTaxValue = 15.75m;
            ExpectedTotalCharge = 115.8m;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent()
                        {
                            Id = "events/1",
                            Country = "some country",
                            TicketPrice = 100,
                            MaximumParticipants = 100,
                            MinimumParticipants = 10
                        };
            using(var session = DocumentStore.OpenSession())
            {
                session.Store(Event);
                session.SaveChanges();
            }

            SetTaxRateForCountry(Event.Country, TaxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            PayPalGatewayCanCreateDelayedPayment(PaypalPayKey);
        }

        [Test]
        public void PledgeIsCreated()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest() { AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/" };
                var action = new MakePledgeAction(TaxResolverMock.Object,
                    PaymentGateway.Object, session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                var _event = session.Load<GroupGivingEvent>(Event.Id);
                Assert.That(_event.Pledges.Count, Is.GreaterThan(0));
            }
        }

        [Test]
        public void PledgeIsSaved()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                var _event = session.Load<GroupGivingEvent>(Event.Id);
                Assert.That(_event.Pledges.Any(p => p.Payments.Any(pmt=>pmt.TransactionId.Equals(PaypalPayKey))));
            }
        }

        [Test]
        public void SubTotalIsTicketPriceTimesAttendeeCount()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);

                var _event = session.Load<GroupGivingEvent>(Event.Id);
                var pledge = _event.Pledges.LastOrDefault();

                Assert.That(pledge.SubTotal, Is.EqualTo(Event.TicketPrice));
            }
        }

        [Test]
        [Ignore("Tax logic is undefined")]
        public void TaxIsAddedToSubTotal()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                var _event = session.Load<GroupGivingEvent>(Event.Id);
                var pledge = _event.Pledges.LastOrDefault();

                Assert.That(pledge.Tax, Is.EqualTo(ExpectedTaxValue));
            }
        }

        [Test]
        [Ignore("Tax logic is undefined")]
        public void TotalIsSubTotalPlusTax()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                var _event = session.Load<GroupGivingEvent>(Event.Id);
                var pledge = _event.Pledges.LastOrDefault();

                Assert.That(pledge.Total, Is.EqualTo(ExpectedTotalCharge));
            }
        }

        [Test]
        public void StatusIsUnpaid()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                var _event = session.Load<GroupGivingEvent>(Event.Id);
                var pledge = _event.Pledges.LastOrDefault();

                Assert.That(pledge.Payments.First().PaymentStatus, Is.EqualTo(PaymentStatus.Created));
                Assert.That(pledge.Paid, Is.False);
            }
        }

        [Test]
        public void ResultContainsGatewayResponse()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);

                Assert.That(_result.TransactionId, Is.EqualTo(PaypalPayKey));
            }
        }

        [Test]
        public void ResultSuccessful()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                Assert.That(_result.Succeeded, Is.True);
            }
        }
    }

    [TestFixture]
    public class MakingAPledgeWithInvalidRequest : PledgeTestsBase
    {
        private CreatePledgeActionResult _result;
        protected decimal ExpectedTotalCharge;
        protected decimal ExpectedTaxValue;
        protected string[] Attendee;
        protected decimal TaxRate;
        private Mock<IIdentity> _userIdentity = new Mock<IIdentity>();

        [SetUp]
        public void SetUp()
        {
            TaxRate = 0.15m;
            Attendee = new string[] { "some guy" };
            ExpectedTaxValue = 15;
            ExpectedTotalCharge = 115;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, MaximumParticipants = 100, MinimumParticipants = 10};

            using (var session = DocumentStore.OpenSession())
            {
                session.Store(Event);
                session.SaveChanges();
            }

            PaymentGatewayReturnsAnErrorWhenMakingDelayedPaymentRequest();
            SetTaxRateForCountry(Event.Country, TaxRate);
            PaymentGatewayReturnsPaymentDetailsForTransactionId(PaymentStatus.Created);
        }

        [Test]
        public void PledgeActionSucceededIsFalse()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var organiserAccount = ValidAccount();
                var request = new MakePledgeRequest()
                                  {AttendeeNames = Attendee, WebsiteUrlBase = "http://ticketmuffin.com/"};
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                _result = action.Attempt(Event.Id, organiserAccount, request);
                Assert.That(_result.Succeeded, Is.False);
            }
        }
    }

    [TestFixture]
    public class PledgePaymentCompleted : PledgeTestsBase
    {
        protected decimal _expectedTotalCharge;
        protected decimal _expectedTaxValue;
        protected string[] _attendee;
        protected decimal _taxRate;
        private CreatePledgeActionResult _result;
        private IDocumentSession _documentSession;
        private Mock<IIdentity> _userIdentity = new Mock<IIdentity>();

        [SetUp]
        public virtual void SetUp()
        {
            _taxRate = 0.15m;
            _attendee = new string[] { "some guy" };
            _expectedTaxValue = 15.75m;
            _expectedTotalCharge = 115.8m;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent()
            {
                Id = "events/1",
                Country = "some country",
                TicketPrice = 100,
                MaximumParticipants = 100,
                MinimumParticipants = 10
            };

            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            PayPalGatewayCanCreateDelayedPayment(PaypalPayKey);
            PaymentGatewayReturnsPaymentDetailsForTransactionId(PaymentStatus.AuthorisedUnsettled);

            _documentSession = DocumentStore.OpenSession();
            _documentSession.Store(Event);
            _documentSession.SaveChanges();

            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee, WebsiteUrlBase = "http://ticketmuffin.com/" };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, _documentSession, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
        }

        [TearDown]
        public void TearDown()
        {
            _documentSession.Dispose();
        }

        [Test]
        public void PaymentStatusSetToPaidPendingReconciliation()
        {
            var request = new SettlePledgeRequest(){TransactionId = PaypalPayKey};
            var action = new ConfirmPledgePaymentAction(PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);
            action.ConfirmPayment(Event, request);

            var _event = _documentSession.Load<GroupGivingEvent>(Event.Id);
            var pledge = _event.Pledges.LastOrDefault();

            Assert.That(pledge.Payments.First().PaymentStatus, Is.EqualTo(PaymentStatus.AuthorisedUnsettled));
            Assert.That(pledge.Paid, Is.True);
        }

        [Test]
        public void PledgeNotInUnpaidState_ThrowInvalidOperationException()
        {
            var _event = _documentSession.Load<GroupGivingEvent>(Event.Id);
            var pledge = _event.Pledges.LastOrDefault();
            pledge.Payments.Add(new Payment(){PaymentStatus = PaymentStatus.AuthorisedUnsettled});

            var request = new SettlePledgeRequest() { TransactionId = PaypalPayKey };
            var action = new ConfirmPledgePaymentAction(PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);

            Assert.Throws<InvalidOperationException>(() => action.ConfirmPayment(Event, request));
        }

        [Test]
        public void PledgeDateSetToNow()
        {
            var request = new SettlePledgeRequest() { TransactionId = PaypalPayKey };
            var action = new ConfirmPledgePaymentAction(PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);
            action.ConfirmPayment(Event, request);

            var _event = _documentSession.Load<GroupGivingEvent>(Event.Id);
            var pledge = _event.Pledges.LastOrDefault();
            Assert.That(pledge.DatePledged, Is.GreaterThan(DateTime.Now.AddSeconds(-1)));
            Assert.That(pledge.DatePledged, Is.LessThan(DateTime.Now.AddSeconds(1)));
        }
    }

    [TestFixture]
    public class MakingAPledgeForAnEventWhichIsNearlyFull : PledgeTestsBase
    {
        protected decimal _expectedTotalCharge;
        protected decimal _expectedTaxValue;
        protected string[] _attendee;
        protected decimal _taxRate;
        private Mock<IIdentity> _userIdentity = new Mock<IIdentity>();

        [SetUp]
        public void SetUp()
        {
            _taxRate = 0.15m;
            _attendee = new string[] { "some guy" };
            _expectedTaxValue = 15;
            _expectedTotalCharge = 115;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, 
                MaximumParticipants = 7,
                Pledges = new List<EventPledge>()
                              {
                                  new EventPledge()
                                      {
                                          Payments = new List<Payment>(new[]{new Payment(){PaymentStatus = PaymentStatus.Settled}, }),
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          }
                                      }
                              }};

            using (var session = DocumentStore.OpenSession())
            {
                session.Store(Event);
                session.SaveChanges();
            }

            PaymentRequestIsSuccessful(PaypalPayKey);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
        }

        [Test]
        public void ToManyAttendeesFails()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var request = new MakePledgeRequest()
                                  {
                                      AttendeeNames = new[] {"attendee1", "attendee2", "attendee3", "attendee4"},
                                      WebsiteUrlBase = "http://ticketmuffin.com/"
                                  };
                var organiserAccount = ValidAccount();
                var action = new MakePledgeAction(TaxResolverMock.Object,
                                                  PaymentGateway.Object,
                                                  session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);

                Assert.Throws<InvalidOperationException>(() => action.Attempt(Event.Id, organiserAccount, request),
                                                         "Number of attendees exceeded");
            }
        }
    }

    [TestFixture]
    public class MakingAPledgeForAnEventWhichHasNearlyReachedMinimumPledges : PledgeTestsBase
    {
        protected decimal _expectedTotalCharge;
        protected decimal _expectedTaxValue;
        protected string[] _attendee;
        protected decimal _taxRate;
        private Mock<IIdentity> _userIdentity = new Mock<IIdentity>();

        [SetUp]
        public void SetUp()
        {
            _taxRate = 0.15m;
            _attendee = new string[] { "some guy" };
            _expectedTaxValue = 15;
            _expectedTotalCharge = 115;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent()
            {
                Id = "events/1",
                Title = "test event",
                Country = "some country",
                TicketPrice = 100,
                MinimumParticipants = 7,
                MaximumParticipants = 100,
                Pledges = new List<EventPledge>()
                              {
                                  new EventPledge()
                                      {
                                          Payments = new List<Payment>(new[]{new Payment(){PaymentStatus=PaymentStatus.Settled,TransactionId="1234"}, }),
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          },
                                                          PaymentGatewayHistory = new List<DialogueHistoryEntry>()
                                      },
                                      new EventPledge()
                                          {
                                          Payments = new List<Payment>(new[]{new Payment(){PaymentStatus=PaymentStatus.Created,TransactionId="12345"}, }),
                                              Attendees = new List<EventPledgeAttendee>()
                                                              {
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                              },
                                                          PaymentGatewayHistory = new List<DialogueHistoryEntry>()
                                          }
                              }
            };

            using (var session = DocumentStore.OpenSession())
            {
                session.Store(Event);
                session.SaveChanges();
            }

            PaymentRequestIsSuccessful(PaypalPayKey);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            PaymentGatewayReturnsPaymentDetailsForTransactionId(PaymentStatus.Created);
            PayPalGatewayCanCreateDelayedPayment(PaypalPayKey);
        }

        [Test]
        public void MinimumAttendeesReachedThenEventIsOn()
        {
            var request = new MakePledgeRequest()
            {
                AttendeeNames = new[] { "attendee1", "attendee2", "attendee3" },
                WebsiteUrlBase = "http://ticketmuffin.com/"
            };

            var organiserAccount = ValidAccount();

            using (var session = DocumentStore.OpenSession())
            {
                var pledgeAction = new MakePledgeAction(TaxResolverMock.Object, PaymentGateway.Object,
                                                        session, OrderNumberGenerator.Object, _userIdentity.Object, AccountService.Object);
                pledgeAction.Attempt(Event.Id, organiserAccount, request);
                var @event = session.Load<GroupGivingEvent>(Event.Id);

                var completeAction =
                    new ConfirmPledgePaymentAction(PaymentGateway.Object, AccountService.Object,
                                                   EmailRelayService.Object);
                completeAction.ConfirmPayment(@event, new SettlePledgeRequest() {TransactionId = PaypalPayKey});


                Assert.That(@event.IsOn, Is.True);
            }
        }

        [Test]
        public void WhenGeneratingAnOrderNumber_NumberIs5DigitsLongAndOnlyNumbers()
        {
            var orderNumberGenerator = new OrderNumberGenerator();
            string number = orderNumberGenerator.Generate(
                new GroupGivingEvent()
                    {
                        Pledges = new List<EventPledge>()
                            {
                                new EventPledge()
                                    {
                                        Attendees = new List<EventPledgeAttendee>()
                                            {
                                                new EventPledgeAttendee("chris"){TicketNumber="00001"},
                                                new EventPledgeAttendee("john"){TicketNumber="00002"},
                                                new EventPledgeAttendee("dave"){TicketNumber="00003"},
                                            }
                                    }
                            }
                    });

            Assert.That(number, Is.StringMatching("00004"));

        }
    }
}
