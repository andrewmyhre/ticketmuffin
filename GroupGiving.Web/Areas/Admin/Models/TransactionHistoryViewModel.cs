using System.Collections.Generic;
using GroupGiving.PayPal.Model;

namespace GroupGiving.Web.Areas.Admin.Models
{
    public class TransactionHistoryViewModel
    {
        public List<DialogueHistoryEntry> Messages { get; set; }

        public int EventId { get; set; }

        public string OrderNumber { get; set; }
    }
}