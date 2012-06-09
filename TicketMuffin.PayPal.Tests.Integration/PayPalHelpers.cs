using TicketMuffin.PayPal.Configuration;

namespace TicketMuffin.PayPal.Tests.Integration
{
    public static class PayPalHelpers
    {
        public static AdaptiveAccountsConfiguration SandboxConfiguration()
        {
            return new AdaptiveAccountsConfiguration()
                       {
                           FailureCallbackUrl = "http://somedomain.com/failure",
                           SuccessCallbackUrl = "http://somedomain.com/success",
                           ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                           ApiPassword = "1321277160",
                           ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                           ApiVersion = "1.1.0",
                           SandboxApiBaseUrl = "https://svcs.sandbox.paypal.com",
                           SandboxApplicationId = "APP-80W284485P519543T",
                           SandboxMode = true,
                           SandboxPayFlowProPaymentPage = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}",
                           RequestDataBinding = "XML",
                           ResponseDataBinding = "XML",
                           PayPalAccountEmail = "something@something.com"
                       };
        }
        public static AdaptiveAccountsConfiguration ProductionConfiguration()
        {
            return new AdaptiveAccountsConfiguration()
            {
                FailureCallbackUrl = "http://somedomain.com/failure",
                SuccessCallbackUrl = "http://somedomain.com/success",
                ApiUsername = "Muffin_1321277131_biz_api1.gmail.com",
                ApiPassword = "1321277160",
                ApiSignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ANDzgYINyuYs1FQZcsN1DSKkJexD",
                ApiVersion = "1.1.0",
                SandboxApiBaseUrl = "https://svcs.paypal.com",
                SandboxApplicationId = "APP-80W284485P519543T",
                SandboxMode = false,
                SandboxPayFlowProPaymentPage = "https://www.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}",
                RequestDataBinding = "XML",
                ResponseDataBinding = "XML",
                PayPalAccountEmail = "Muffin_1321277131_biz@gmail.com"
            };
        }
    }
}