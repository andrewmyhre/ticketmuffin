using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Domain
{
    public class Payment
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }

        public string PaymentGatewayName { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
    }
}