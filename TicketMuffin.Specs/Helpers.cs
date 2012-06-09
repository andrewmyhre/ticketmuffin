using System.Collections.Generic;
using Moq;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Specs
{
    public class Helpers
    {
        public static void CreateDelayedPaymentSucceeds(Mock<IPaymentGateway> mock)
        {
            mock
                .Setup(x => x.CreateDelayedPayment(It.IsAny<PaymentGatewayRequest>()))
                .Returns(new PaymentGatewayResponse() { PaymentExecStatus = "CREATED" });
        }

        public static void NoTax(Mock<ITaxAmountResolver> mock)
        {
            mock.Setup(x => x.LookupTax(It.IsAny<string>())).Returns(0);
        }

        public static Account TestAccount()
        {
            return new Account()
                       {
                           FirstName = "test",
                           LastName = "test",
                           Email = "test@test.com",
                           AccountType = AccountType.Individual
                       };
        }

        public static IEnumerable<EventPledgeAttendee> Attendees(int count)
        {
            for (int i = 0; i < count; i++)
                yield return new EventPledgeAttendee(i.ToString());
        }

        public static void PaymentGatewayReturnsSuccessful(Mock<IPaymentGateway> paymentGateway)
        {
            paymentGateway
                .Setup(x => x.RetrievePaymentDetails(It.IsAny<string>()))
                .Returns(new PaymentDetailsResponse()
                             {
                                 Raw = new DialogueHistoryEntry("request","response"),
                                 status = "CREATED"
                             });
        }
    }
}