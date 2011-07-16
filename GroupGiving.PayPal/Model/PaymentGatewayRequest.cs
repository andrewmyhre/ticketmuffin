using System;

namespace GroupGiving.PayPal.Model
{
    public class PaymentGatewayRequest
    {
        public decimal Amount { get; set; }

        public string OrderMemo { get; set; }

        public string FailureCallbackUrl { get; set; }

        public string SuccessCallbackUrl { get; set; }
    }
}