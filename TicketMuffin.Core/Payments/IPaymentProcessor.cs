namespace TicketMuffin.Core.Payments
{
    public interface IPaymentProcessor
    {
        IChargeResponse CreateAndCaptureCharge();
    }
}