using System;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Areas.Admin.Models
{
    public class TransactionHistoryEntry
    {
        public string EventId { get; set; }
        public string OrderNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string AccountEmailAddress { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}