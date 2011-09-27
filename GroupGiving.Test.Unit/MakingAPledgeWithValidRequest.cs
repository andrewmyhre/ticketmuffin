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
            _expectedTaxValue = 15;
            _expectedTotalCharge = 115;
            _paypalPayKey = "12345";
            _event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, MaximumParticipants = 100, MinimumParticipants = 10};

            SetDummyPaypalConfiguration();
            AnyPayPalRequestReturnsPayKey(_paypalPayKey);
            EventRepositoryReturns(_event);
            SetTaxRateForCountry(_event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();

            var request = new MakePledgeRequest() { AttendeeNames = _attendee };
            var action = new MakePledgeAction(taxResolverMock.Object, eventRepositoryMock.Object, 
                paypalService.Object, _paypalConfiguration.Object);
            _result = action.Attempt(_event, new Account(), request);
        }

        [Test]
        public void PledgeIsCreated()
        {
            Assert.That(_event.Pledges, Has.Count.EqualTo(1));
        }

        [Test]
        public void PledgeIsSaved()
        {
            eventRepositoryMock.Verify();
        }

        [Test]
        public void SubTotalIsTicketPriceTimesAttendeeCount()
        {
            var pledge = _event.Pledges.LastOrDefault();

            Assert.That(pledge.SubTotal, Is.EqualTo(_event.TicketPrice));
        }

        [Test]
        public void TaxIsAddedToSubTotal()
        {
            var pledge = _event.Pledges.LastOrDefault();

            Assert.That(pledge.Tax, Is.EqualTo(_expectedTaxValue));
        }

        [Test]
        public void TotalIsSubTotalPlusTax()
        {
            var pledge = _event.Pledges.LastOrDefault();

            Assert.That(pledge.Total, Is.EqualTo(_expectedTotalCharge));
        }

        [Test]
        public void StatusIsUnpaid()
        {
            var pledge = _event.Pledges.LastOrDefault();

            Assert.That(pledge.PaymentStatus, Is.EqualTo(PaymentStatus.Unpaid));
        }

        [Test]
        public void PaymentGatewayRequestIsMade()
        {
            paypalService.Verify(m=>m.MakeRequest(It.IsAny<PaymentGatewayRequest>()));
        }

        [Test]
        public void ResultContainsGatewayResponse()
        {
            Assert.That(_result.GatewayResponse, Is.Not.Null);
            Assert.That(_result.GatewayResponse.TransactionId, Is.EqualTo(_paypalPayKey));
        }

        [Test]
        public void ResultSuccessful()
        {
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
            _paypalPayKey = "12345";
            _event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, MaximumParticipants = 100, MinimumParticipants = 10};

            PaypalGatewayReturnsAnErrorWhenMakingPaymentRequest();
            EventRepositoryReturns(_event);
            SetTaxRateForCountry(_event.Country, TaxRate);
            EventRepositoryStoresEventWithVerification();

            var request = new MakePledgeRequest() { AttendeeNames = Attendee };
            var action = new MakePledgeAction(taxResolverMock.Object, eventRepositoryMock.Object, 
                paypalService.Object, _paypalConfiguration.Object);
            _result = action.Attempt(_event, new Account(), request);
        }

        [Test]
        public void PledgeActionSucceededIsFalse()
        {
            Assert.That(_result.Succeeded, Is.False);
        }
    }

    [TestFixture]
    public class PledgePaymentCompleted : MakingAPledgeWithValidRequest
    {
        [SetUp]
        public void Setup()
        {
            base.SetUp();
        }

        [Test]
        public void PaymentStatusSetToPaidPendingReconciliation()
        {
            var request = new SettlePledgeRequest(){PayPalPayKey = _paypalPayKey};
            var action = new ConfirmPledgePaymentAction(eventRepositoryMock.Object);
            action.ConfirmPayment(request);

            var pledge = _event.Pledges.LastOrDefault();
            Assert.That(pledge.PaymentStatus, Is.EqualTo(PaymentStatus.PaidPendingReconciliation));
        }

        [Test]
        public void PledgeNotInUnpaidState_ThrowInvalidOperationException()
        {
            var pledge = _event.Pledges.LastOrDefault();
            pledge.PaymentStatus = PaymentStatus.PaidPendingReconciliation;

            var request = new SettlePledgeRequest() { PayPalPayKey = _paypalPayKey };
            var action = new ConfirmPledgePaymentAction(eventRepositoryMock.Object);

            Assert.Throws<InvalidOperationException>(() => action.ConfirmPayment(request));
        }

        [Test]
        public void PledgeDateSetToNow()
        {
            var request = new SettlePledgeRequest() { PayPalPayKey = _paypalPayKey };
            var action = new ConfirmPledgePaymentAction(eventRepositoryMock.Object);
            action.ConfirmPayment(request);

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

        [SetUp]
        public void SetUp()
        {
            _taxRate = 0.15m;
            _attendee = new string[] { "some guy" };
            _expectedTaxValue = 15;
            _expectedTotalCharge = 115;
            _paypalPayKey = "12345";
            _event = new GroupGivingEvent() { Id = "events/1", Country = "some country", TicketPrice = 100, 
                MaximumParticipants = 7,
                Pledges = new List<EventPledge>()
                              {
                                  new EventPledge()
                                      {
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          }
                                      }
                              }};

            SetDummyPaypalConfiguration();
            AnyPayPalRequestReturnsPayKey(_paypalPayKey);
            EventRepositoryReturns(_event);
            SetTaxRateForCountry(_event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
        }

        [Test]
        public void ToManyAttendeesFails()
        {
            var request = new MakePledgeRequest()
            {
                AttendeeNames = new[] { "attendee1", "attendee2", "attendee3" }
            };
            var action = new MakePledgeAction(taxResolverMock.Object, eventRepositoryMock.Object, 
                paypalService.Object, _paypalConfiguration.Object);

            Assert.Throws<InvalidOperationException>(() => action.Attempt(_event, new Account(), request), "Number of attendees exceeded");
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
            _paypalPayKey = "12345";
            _event = new GroupGivingEvent()
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
                                          Attendees = new List<EventPledgeAttendee>()
                                                          {
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"},
                                                              new EventPledgeAttendee(){FullName="attendee"}
                                                          }
                                      }
                              }
            };

            SetDummyPaypalConfiguration();
            AnyPayPalRequestReturnsPayKey(_paypalPayKey);
            EventRepositoryReturns(_event);
            SetTaxRateForCountry(_event.Country, _taxRate);
            PayPalGatewayReturnsTransactionIdWithVerification();
            EventRepositoryStoresEventWithVerification();
        }

        [Test]
        public void ToManyAttendeesFails()
        {
            var request = new MakePledgeRequest()
            {
                AttendeeNames = new[] { "attendee1", "attendee2", "attendee3" }
            };
            var action = new MakePledgeAction(taxResolverMock.Object, eventRepositoryMock.Object,
                paypalService.Object, _paypalConfiguration.Object);

            var result = action.Attempt(_event, new Account(), request);

            Assert.That(_event.IsOn, Is.True);
        }
    }
}
