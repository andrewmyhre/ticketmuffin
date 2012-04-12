using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Services;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class PaypalAdaptiveAccountsService : IAdaptiveAccountsService
    {
        private readonly IApiClient _apiClient;
        private readonly AdaptiveAccountsConfiguration _configuration;

        public PaypalAdaptiveAccountsService(
            IApiClient apiClient,
            AdaptiveAccountsConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        public bool AccountIsVerified(string email, string firstname, string lastname)
        {
            var getVerifiedStatusRequest = new GetVerifiedStatusRequest(_configuration)
                                               {
                                                   FirstName=firstname,
                                                   LastName=lastname,
                                                   EmailAddress=email
                                               };

            try
            {
                return _apiClient.VerifyAccount(getVerifiedStatusRequest).Verified;
            } catch
            {
                return false;
            }
        }
    }
}
