namespace TicketMuffin.PayPal.Model
{
    public interface IPayPalRequest
    {
        RequestEnvelope RequestEnvelope { get; set; }
        ClientDetails ClientDetails { get; set; }
    }

}