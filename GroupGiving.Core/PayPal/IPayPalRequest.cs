namespace GroupGiving.Core.PayPal
{
    public interface IPayPalRequest
    {
        RequestEnvelope RequestEnvelope { get; set; }
        ClientDetails ClientDetails { get; set; }
    }

}