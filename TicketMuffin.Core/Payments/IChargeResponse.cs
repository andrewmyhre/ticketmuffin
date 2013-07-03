namespace TicketMuffin.Core.Payments
{
    public interface IChargeResponse
    {
        bool Success { get; set; }
    }
}