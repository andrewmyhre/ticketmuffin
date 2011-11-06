namespace GroupGiving.Core.Dto
{
    public class PaymentGatewayResponse : IPaymentGatewayResponse
    {
        public IResponseEnvelope ResponseEnvelope { get; set; }

        public string payKey { get; set; }

        public string PaymentExecStatus { get; set; }

        public string PaymentPageUrl { get; set; }

        public ResponseError[] Errors { get; set; }
    }
}