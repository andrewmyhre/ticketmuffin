namespace TicketMuffin.Core.Payments
{
    public interface IPaymentGateway
    {
        object CreateDelayedPayment(object request);
        PaymentDetailsResponse RetrievePaymentDetails(string transactionId);
        IPaymentRefundResponse Refund(string transactionId, decimal amount, string receiverId);
        IPaymentCaptureResponse CapturePayment(string transactionId);
        IPaymentAuthoriseResponse AuthoriseCharge(decimal amount, string currencyCode, string paymentMemo, string recipientId, bool capture=false);
        PaymentCreationResponse CreatePayment(string memo, string iso4217Alpha3Code, string successUrl, string failureUrl, Receiver[] receivers);
        string Name { get; }
    }

    public class Receiver
    {
        public decimal Amount { get; set; }
        public string EmailAddress { get; set; }
        public bool Primary { get; set; }
    }

    public class PaymentCreationResponse : IPaymentAction
    {
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public TransactionDiagnostics Diagnostics { get; set; }
        public bool Successful { get; set; }
    }

    public interface IPaymentAuthoriseResponse : IPaymentAction
    {
        PaymentStatus Status { get; set; }
        string TransactionId { get; set; }
    }

    public class PaymentDetailsResponse : IPaymentAction
    {
        public PaymentStatus PaymentStatus { get; set; }
        public string SenderId { get; set; }
        public TransactionDiagnostics Diagnostics { get; set; }
        public bool Successful { get; set; }
    }
    public enum PaymentStatus
    {
        Unpaid,
        Unsettled,
        Settled,
        Refunded
    }
}