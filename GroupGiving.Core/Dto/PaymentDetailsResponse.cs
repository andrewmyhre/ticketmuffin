namespace GroupGiving.Core.Dto
{
    public class PaymentDetailsResponse
    {
        public string SenderEmailAddress { get; set; }

        public string Status { get; set; }

        public object RawResponse { get; set; }
    }
}