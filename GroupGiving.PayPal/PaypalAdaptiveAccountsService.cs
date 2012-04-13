using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Dto;
using GroupGiving.Core.PayPal;
using GroupGiving.Core.Services;

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

        public GetVerifiedStatusResponse AccountIsVerified(string email, string firstname, string lastname)
        {
            var getVerifiedStatusRequest = new GetVerifiedStatusRequest(_configuration)
                                               {
                                                   FirstName=firstname,
                                                   LastName=lastname,
                                                   EmailAddress=email
                                               };

            var response = _apiClient.VerifyAccount(getVerifiedStatusRequest);

            return new GetVerifiedStatusResponse()
                       {
                           AccountStatus = response.AccountStatus,
                           Success=true
                       };
        }
    }
}
