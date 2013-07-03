using TicketMuffin.Core.Payments;

namespace TicketMuffin.PayPal
{
    public class PayPalParallelPaymentProcessor : IPaymentProcessor
    {
        public IChargeResponse CreateAndCaptureCharge()
        {
            throw new System.NotImplementedException();
        }
    }
}