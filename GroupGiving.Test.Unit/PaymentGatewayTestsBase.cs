using GroupGiving.Core.Configuration;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;

namespace GroupGiving.Test.Unit
{
    public class PaymentGatewayTestsBase : InMemoryStoreTest
    {
        protected static PaymentGatewayRequest ValidPaymentGatewayRequest()
        {
            return new PaymentGatewayRequest()
                       {
                           Amount=1,
                           FailureCallbackUrl = "http://somedomain.com/failure",
                           SuccessCallbackUrl = "http://somedomain.com/success",
                           OrderMemo = "Logs of wood"
                       };
        }

        protected static ISiteConfiguration ValidPayPalConfiguration()
        {
            return new SiteConfiguration()
                       {
                           AdaptiveAccountsConfiguration= new AdaptiveAccountsConfiguration()
                                                              {
                                                                  LivePayFlowProPaymentPage = "http://somedomain.com/pay",
                                                                  SandboxMode = false,
                                                                  ApiUsername = "12345",
                                                                  ApiPassword = "12345",
                                                                  ApiSignature = "12345"
                                                              }
                       };
        }
    }
}