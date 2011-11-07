using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Actions.SettlePledge;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using Moq;
using NUnit.Framework;

namespace GroupGiving.Test.Unit.Pledging
{
    [TestFixture]
    [Category("Pledging")]
    public class MakingAPledgeWithValidRequest : PledgeTestsBase
    {
        protected decimal _expectedTotalCharge;
        protected decimal _expectedTaxValue;
        protected string[] _attendee;
        protected decimal _taxRate;
        private CreatePledgeActionResult _result;

        [SetUp]
        public virtual void SetUp()
        {
            _taxRate = 0.15m;
            _attendee = new string[] { "some guy" };
            _expectedTaxValue = 15.75m;
            _expectedTotalCharge = 115.8m;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent() { Id = "events/1", Country = "some country", 
                TicketPrice = 100, 
                MaximumParticipants = 100, MinimumParticipants = 10};

            SetDummyPaypalConfiguration();
            EventRepositoryReturns(Event);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
            SetUpDocumentStore();
            SessionLoadsAccount(ValidAccount());
        }

        [Test]
        public void PledgeIsCreated()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            Assert.That(Event.Pledges, Has.Count.EqualTo(1));
        }

        [Test]
        public void PledgeIsSaved()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            EventRepositoryMock.Verify();
        }

        [Test]
        public void SubTotalIsTicketPriceTimesAttendeeCount()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            var pledge = Event.Pledges.LastOrDefault();

            Assert.That(pledge.SubTotal, Is.EqualTo(Event.TicketPrice));
        }

        [Test]
        public void TaxIsAddedToSubTotal()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            var pledge = Event.Pledges.LastOrDefault();

            Assert.That(pledge.Tax, Is.EqualTo(_expectedTaxValue));
        }

        [Test]
        public void TotalIsSubTotalPlusTax()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            var pledge = Event.Pledges.LastOrDefault();

            Assert.That(pledge.Total, Is.EqualTo(_expectedTotalCharge));
        }

        [Test]
        public void StatusIsUnpaid()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            var pledge = Event.Pledges.LastOrDefault();

            Assert.That(pledge.PaymentStatus, Is.EqualTo(PaymentStatus.Unpaid));
            Assert.That(pledge.Paid, Is.False);
        }

        [Test]
        public void ResultContainsGatewayResponse()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            Assert.That(_result.GatewayResponse, Is.Not.Null);
            Assert.That(_result.GatewayResponse.payKey, Is.EqualTo(PaypalPayKey));
        }

        [Test]
        public void ResultSuccessful()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            Assert.That(_result.Succeeded, Is.True);
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

        [SetUp]
        public void SetUp()
        {
            TaxRate = 0.15m;
            Attendee = new string[] { "some guy" };
            ExpectedTaxValue = 15;
            ExpectedTotalCharge = 115;
            PaypalPayKey = "12345";
            Event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, MaximumParticipants = 100, MinimumParticipants = 10};

            PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest();
            EventRepositoryReturns(Event);
            SetTaxRateForCountry(Event.Country, TaxRate);
            EventRepositoryStoresEventWithVerification();

        }

        [Test]
        public void PledgeActionSucceededIsFalse()
        {
            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = Attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
            Assert.That(_result.Succeeded, Is.False);
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

            SetDummyPaypalConfiguration();
            EventRepositoryReturns(Event);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
            SetUpDocumentStore();
            SessionLoadsAccount(ValidAccount());

            var organiserAccount = ValidAccount();
            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);
            _result = action.Attempt(Event.Id, organiserAccount, request);
        }

        [Test]
        public void PaymentStatusSetToPaidPendingReconciliation()
        {
            var request = new SettlePledgeRequest(){PayPalPayKey = PaypalPayKey};
            var action = new ConfirmPledgePaymentAction(EventRepositoryMock.Object, PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);
            action.ConfirmPayment(request);

            var pledge = Event.Pledges.LastOrDefault();
            Assert.That(pledge.PaymentStatus, Is.EqualTo(PaymentStatus.PaidPendingReconciliation));
            Assert.That(pledge.Paid, Is.True);
        }

        [Test]
        public void PledgeNotInUnpaidState_ThrowInvalidOperationException()
        {
            var pledge = Event.Pledges.LastOrDefault();
            pledge.PaymentStatus = PaymentStatus.PaidPendingReconciliation;

            var request = new SettlePledgeRequest() { PayPalPayKey = PaypalPayKey };
            var action = new ConfirmPledgePaymentAction(EventRepositoryMock.Object, PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);

            Assert.Throws<InvalidOperationException>(() => action.ConfirmPayment(request));
        }

        [Test]
        public void PledgeDateSetToNow()
        {
            var request = new SettlePledgeRequest() { PayPalPayKey = PaypalPayKey };
            var action = new ConfirmPledgePaymentAction(EventRepositoryMock.Object, PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);
            action.ConfirmPayment(request);

            var pledge = Event.Pledges.LastOrDefault();
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
                                          Paid=true,
                                          PaymentStatus = PaymentStatus.Reconciled,
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          }
                                      }
                              }};

            SetDummyPaypalConfiguration();
            PaymentRequestIsSuccessful(PaypalPayKey);
            EventRepositoryReturns(Event);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
        }

        [Test]
        public void ToManyAttendeesFails()
        {
            var request = new MakePledgeRequest()
            {
                AttendeeNames = new[] { "attendee1", "attendee2", "attendee3", "attendee4" }
            };
            var organiserAccount = ValidAccount();
            var action = new MakePledgeAction(TaxResolverMock.Object,
                PaymentGateway.Object, PaypalConfiguration.Object, DocumentStore.Object);

            Assert.Throws<InvalidOperationException>(() => action.Attempt(Event.Id, organiserAccount, request), "Number of attendees exceeded");
        }
    }

    [TestFixture]
    public class MakingAPledgeForAnEventWhichHasNearlyReachedMinimumPledges : PledgeTestsBase
    {
        protected decimal _expectedTotalCharge;
        protected decimal _expectedTaxValue;
        protected string[] _attendee;
        protected decimal _taxRate;

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
                Country = "some country",
                TicketPrice = 100,
                MinimumParticipants = 7,
                MaximumParticipants = 100,
                Pledges = new List<EventPledge>()
                              {
                                  new EventPledge()
                                      {
                                          Paid=true,
                                          PaymentStatus = PaymentStatus.Reconciled,
                                          TransactionId = "1234",
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          }
                                      },
                                      new EventPledge()
                                          {
                                              Paid=false,
                                              PaymentStatus = PaymentStatus.Unpaid,
                                              TransactionId = PaypalPayKey,
                                              Attendees = new List<EventPledgeAttendee>()
                                                              {
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                                  new EventPledgeAttendee() {FullName="attendee"},
                                                              }
                                          }
                              }
            };

            SetDummyPaypalConfiguration();
            PaymentRequestIsSuccessful(PaypalPayKey);
            EventRepositoryReturns(Event);
            SetTaxRateForCountry(Event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
        }

        [Test]
        public void MinimumAttendeesReachedThenEventIsOn()
        {
            var request = new MakePledgeRequest()
            {
                AttendeeNames = new[] { "attendee1", "attendee2", "attendee3" }
            };

            var organiserAccount = ValidAccount();

            var completeAction =
                new ConfirmPledgePaymentAction(EventRepositoryMock.Object, PaymentGateway.Object, AccountService.Object, EmailRelayService.Object);
            completeAction.ConfirmPayment(new SettlePledgeRequest() { PayPalPayKey = PaypalPayKey });


            Assert.That(Event.IsOn, Is.True);
        }
    }
}
