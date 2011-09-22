using GroupGiving.PayPal.AdaptiveAccounts;
using GroupGiving.PayPal.Configuration;
using GroupGiving.Web.Areas.Api.Controllers;
using PayPal.Platform.SDK;
using PayPal.Services.Private.AA;
using AdaptiveAccounts = PayPal.Platform.SDK.AdaptiveAccounts;

namespace GroupGiving.Web.Code
{
    public class PaypalAccountService
    {
        private readonly PaypalAdaptiveAccountsConfigurationSection _configuration;

        public PaypalAccountService(PaypalAdaptiveAccountsConfigurationSection configuration)
        {
            _configuration = configuration;
        }

        public VerifyPaypalAccountResponse VerifyPaypalAccount(VerifyPaypalAccountRequest request)
        {
            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(_configuration);

            GetVerifiedStatusRequest getVerifiedStatusRequest = new GetVerifiedStatusRequest();
            getVerifiedStatusRequest.emailAddress = request.Email;
            getVerifiedStatusRequest.firstName = request.FirstName;
            getVerifiedStatusRequest.lastName = request.LastName;
            getVerifiedStatusRequest.matchCriteria = "NAME";

            AdaptiveAccounts aa = new AdaptiveAccounts();
            aa.APIProfile = profile;
            var response = new VerifyPaypalAccountResponse();
            try
            {
                var verifyResponse = aa.GetVerifiedStatus(getVerifiedStatusRequest);

                if (verifyResponse == null)
                {
                    response.Success = false;
                    response.AccountStatus = "";
                    return response;
                }
                else if (verifyResponse.responseEnvelope.ack != AckCode.Success)
                {
                    response.Success = false;
                    response.AccountStatus = "";
                    return response;
                }

                response.Success = true;
                response.AccountStatus = verifyResponse.accountStatus;
                return response;
            }
            catch
            {
                response.Success = false;
                response.AccountStatus = "";
                return response;
            }
        }

    }
}