using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
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
            var paymentService = _kernel.Get<IPaymentService>();

            using (var session = documentStore.OpenSession())
            {
                @event = CreateAnEventAndOrganiserAccount(session);
                pledger = CreateAnAccount(session);

                var pledgeService = new PledgeService(session, paymentService);
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

        [Test]
        public void CanGetAPaymentUrlToBuyTickets()
        {
            GroupGivingEvent @event = null;
            Account pledger = null;

            string expectedPaymentPageUrl = RandomString();
            string expectedTransactionId = RandomString();

            var paymentGateway = new Mock<IPaymentGateway>();
            paymentGateway.SetupGet(x => x.Name).Returns("MockPaymentGateway");
            paymentGateway
                .Setup(x=>x.CreatePayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Receiver[]>()))
                .Returns(new PaymentCreationResponse() { PaymentUrl = expectedPaymentPageUrl, TransactionId = expectedTransactionId })
                .Verifiable();

            var documentStore = _kernel.Get<IDocumentStore>();
            _kernel.Unbind<IPaymentGateway>();
            _kernel.Bind<IPaymentGateway>().ToMethod(x => paymentGateway.Object);

            using (var session = documentStore.OpenSession())
            {
                @event = CreateAnEventAndOrganiserAccount(session);
                var organiser = session.Load<Account>(@event.OrganiserId);
                pledger = CreateAnAccount(session);
                var pledge = AddAPledgeToEvent(@event, pledger);

                var paymentService = _kernel.Get<IPaymentService>();
                var paymentUrl = paymentService.GetPaymentUrlForPledge(@event,organiser, pledge);

                paymentGateway.VerifyAll();
                Assert.That(paymentUrl, Is.EqualTo(expectedPaymentPageUrl));
                Assert.That(pledge.Payments, Has.Count.EqualTo(1));
                Assert.That(pledge.Payments.SingleOrDefault(x=>x.TransactionId==expectedTransactionId), Is.Not.Null);
            }
        }

        [Test]
        public void GivenPayingForAPledge_WhenPaymentIsSuccessful_ThePledgeIsMarkedPaid()
        {
            GroupGivingEvent @event = null;
            Account pledger = null;

            string transactionId = "transactionId";
            string expectedPayPalEmail = RandomString();
            var paymentGateway = new Mock<IPaymentGateway>();

            var documentStore = _kernel.Get<IDocumentStore>();
            _kernel.Unbind<IPaymentGateway>();
            _kernel.Bind<IPaymentGateway>().ToMethod(x => paymentGateway.Object);

            using (var session = documentStore.OpenSession())
            {
                @event = CreateAnEventAndOrganiserAccount(session);
                pledger = CreateAnAccount(session);
                var pledge = AddAPledgeToEvent(@event, pledger);
                var payment = AddAPaymentToPledge(pledge, PaymentStatus.Unpaid, transactionId);

                paymentGateway.Setup(x => x.RetrievePaymentDetails(transactionId))
                    .Returns(new PaymentDetailsResponse() { Successful = true, PaymentStatus = PaymentStatus.Unsettled, SenderId = expectedPayPalEmail });

                var pledgeService = _kernel.Get<IPledgeService>();

                pledgeService.ConfirmPaidPledge(@event, pledge, pledger, transactionId);

                Assert.That(pledge.Payments, Has.Count.EqualTo(1));
                var actualPayment = pledge.Payments.First();

                Assert.That(pledger.PaymentGatewayId, Is.EqualTo(expectedPayPalEmail), "pledge service should have updated payer's paypal email address");
                Assert.That(actualPayment.PaymentStatus, Is.EqualTo(PaymentStatus.Unsettled));
                Assert.That(pledge.Paid, Is.True);
            }
        }
    }
}
