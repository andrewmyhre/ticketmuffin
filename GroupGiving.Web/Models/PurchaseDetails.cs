namespace GroupGiving.Web.Models
{
    public class PurchaseDetails
    {
        public string PayPalEmailAddress { get; set; }
        public int Quantity { get; set; }
        public string ShortUrl { get; set; }
        public int EventId { get; set; }
    }
}