using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Test.Integration
{
    [TestFixture]
    public class PaypalPaymentProcessingTests
    {
        private IPaymentProcessor paymentProcessor = null;
        [TestFixtureSetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void CanProcessAPayment()
        {
            var response = paymentProcessor.CreateAndCaptureCharge();
            Assert.That(response.Success, Is.True);
        }
    }
}
