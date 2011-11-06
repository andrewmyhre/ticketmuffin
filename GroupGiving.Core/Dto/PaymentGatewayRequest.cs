namespace GroupGiving.Core.Dto
{
    public class PaymentGatewayRequest
    {
        public decimal Amount { get; set; }
        public string OrderMemo { get; set; }
        public string FailureCallbackUrl { get; set; }
        public string SuccessCallbackUrl { get; set; }
    }
}