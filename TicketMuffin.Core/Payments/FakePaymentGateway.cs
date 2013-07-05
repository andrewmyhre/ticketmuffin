using System;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Payments
{
    public class FakePaymentGateway : IPaymentGateway
    {
        private readonly IDocumentSession _session;

        public FakePaymentGateway(IDocumentSession session)
        {
            _session = session;
        }

        public object CreateDelayedPayment(object request)
        {
            throw new System.NotImplementedException();
        }

        public PaymentDetailsResponse RetrievePaymentDetails(string transactionId)
        {
            throw new System.NotImplementedException();
        }

        public IPaymentRefundResponse Refund(string transactionId, decimal amount, string receiverId)
        {
            throw new System.NotImplementedException();
        }

        public IPaymentCaptureResponse CapturePayment(string transactionId)
        {
            throw new System.NotImplementedException();
        }

        public IPaymentAuthoriseResponse AuthoriseCharge(decimal amount, string currencyCode, string paymentMemo, string recipientId,
                                                         bool capture = false)
        {
            throw new System.NotImplementedException();
        }

        public PaymentCreationResponse CreatePayment(string memo, string iso4217Alpha3Code, string successUrl, string failureUrl,
                                                     Receiver[] receivers)
        {
            var payment = new Payment()
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    PaymentGatewayName = Name,
                    PaymentStatus = PaymentStatus.Unsettled
                };
            _session.Store(payment);
            _session.SaveChanges();
            return new PaymentCreationResponse(){TransactionId = payment.TransactionId, Successful=true, PaymentUrl=""};
        }

        public string Name { get { return "TestGateway"; } }
    }
}