namespace TicketMuffin.Core.Payments
{
    public interface IPaymentGateway
    {
        object CreatePayment(object request);
        object CreateDelayedPayment(object request);
        IPaymentDetailsResponse RetrievePaymentDetails(string transactionId);
        IPaymentRefundResponse Refund(string transactionId, decimal amount, string receiverId);
        IPaymentCaptureResponse CapturePayment(string transactionId);
        IPaymentAuthoriseResponse AuthoriseCharge(decimal amount, string currencyCode, string paymentMemo, string recipientId, bool capture=false);
    }

    public interface IPaymentAuthoriseResponse : IPaymentAction
    {
        PaymentStatus Status { get; set; }
        string TransactionId { get; set; }
    }

    public interface IPaymentDetailsResponse : IPaymentAction
    {
        PaymentStatus Status { get; }
        string SenderId { get; set; }
    }
    public enum PaymentStatus
    {
        Unpaid,
        Unsettled,
        Settled,
        Refunded
    }
}