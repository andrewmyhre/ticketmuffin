namespace TicketMuffin.Core.Domain
{
    public class TransactionHistoryItem
    {
        public string EventId { get; set; }
        public string OrderNumber { get; set; }
        public string TransactionId { get; set; }
        public string PaymentStatus { get; set; }
        public string AccountEmailAddress { get; set; }
        public string AccountName { get; set; }
        public string Notes { get; set; }
        public string FullName { get; set; }
        public string AttendeeName { get; set; }
        public string EventName { get; set; }
        public string EventOrganiser { get; set; }
    }
}