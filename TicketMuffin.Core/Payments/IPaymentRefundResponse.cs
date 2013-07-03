namespace TicketMuffin.Core.Payments
{
    public interface IPaymentAction
    {
        TransactionDiagnostics Diagnostics { get; set; }
        bool Successful { get; set; }
    }
    public interface IPaymentRefundResponse : IPaymentAction
    {
    }

    public class TransactionDiagnostics
    {
        public string RequestContent { get; set; }
        public string ResponseContent { get; set; }
    }
}