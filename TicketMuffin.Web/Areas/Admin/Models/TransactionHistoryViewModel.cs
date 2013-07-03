using System.Collections.Generic;
using TicketMuffin.Core;
using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Web.Areas.Admin.Models
{
    public class TransactionHistoryViewModel
    {
        public List<DialogueHistoryEntry> Messages { get; set; }

        public int EventId { get; set; }

        public string OrderNumber { get; set; }
    }
}