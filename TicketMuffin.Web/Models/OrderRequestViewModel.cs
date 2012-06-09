using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Web.Models
{
    public class OrderRequestViewModel
    {
        public string PayKey { get; set; }

        public string Ack { get; set; }

        public ResponseError Error { get; set; }

        public string PayPalPostUrl { get; set; }
    }
}