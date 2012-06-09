using System.Collections.Generic;

namespace TicketMuffin.PayPal.Model
{
    public class PaymentGatewayRequest
    {
        public decimal Amount { get; set; }
        public string OrderMemo { get; set; }
        public string FailureCallbackUrl { get; set; }
        public string SuccessCallbackUrl { get; set; }
        public List<PaymentRecipient> Recipients { get; set; }

        public ActionTypeEnum ActionType { get; set; }

        public string CurrencyCode { get; set; }

        public enum ActionTypeEnum
        {
            Immediate,
            Delayed
        }
    }

    public class PaymentRecipient
    {
        public PaymentRecipient(string emailAddress, decimal amount, bool primary)
        {
            this.EmailAddress = emailAddress;
            this.AmountToReceive = amount;
            Primary = primary;
        }

        public string EmailAddress { get; set; }
        public decimal AmountToReceive { get; set; }
        public bool Primary { get; set; }
    }
}