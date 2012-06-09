namespace TicketMuffin.PayPal.Model
{
    public class CreatePaypalAccountResponse
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string RedirectUrl { get; set; }
        public string CreateAccountKey { get; set; }
    }
}