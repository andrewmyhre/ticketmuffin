using System;

namespace TicketMuffin.PayPal.Model
{
    public class PaymentGatewayResponse : IPaymentGatewayResponse
    {
        public IResponseEnvelope ResponseEnvelope { get; set; }

        public string payKey { get; set; }

        public string PaymentExecStatus { get; set; }

        public string PaymentPageUrl { get; set; }

        public ResponseError Error { get; set; }

        public object RawResponse { get; set; }
        public Exception ExceptionDetails { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }
    }
}