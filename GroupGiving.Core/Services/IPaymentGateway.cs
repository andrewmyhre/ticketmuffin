using GroupGiving.Core.Dto;

namespace GroupGiving.Core.Services
{
    public interface IPaymentGateway
    {
        IPaymentGatewayResponse MakeRequest(PaymentGatewayRequest request);
    }
}