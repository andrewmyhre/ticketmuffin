using System;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Domain
{
    public class Payment
    {
        public Payment()
        {
            ModifiedDate = DateTime.Now;
        }
        public string Id { get; set; }
        public string TransactionId { get; set; }

        public string PaymentGatewayName { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public decimal Total { get; set; }

        public string SettlementCurrencyCode { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}