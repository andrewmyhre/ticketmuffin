using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;

namespace GroupGiving.Core.Services
{
    public interface IPaymentGateway
    {
        PaymentGatewayResponse CreatePayment(PaymentGatewayRequest request);
        TResponse RetrievePaymentDetails<TRequest, TResponse>(TRequest request);
        TResponse Refund<TRequest, TResponse>(TRequest request);
    }
}