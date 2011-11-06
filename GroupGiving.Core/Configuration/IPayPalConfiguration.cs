namespace GroupGiving.Core.Configuration
{
    public interface IPayPalConfiguration
    {
        string PayPalMerchantUsername { get; set; }
        string PayPalMerchantPassword { get; set; }
        string PayPalMerchantSignature { get; set; }
        string SuccessCallbackUrl { get; set; }
        string FailureCallbackUrl { get; set; }
        string PayFlowProPaymentPage { get; set; }
        string TicketMuffinPayPalAccountEmail { get; }
    }
}