using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Clients;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class AccountsController : ApiControllerBase
    {
        private readonly ISiteConfiguration _siteConfiguration;
        private readonly IApiClient _apiClient;
        //
        // GET: /Api/Accounts/
        public AccountsController(IRepository<GroupGivingEvent> eventRepository, ISiteConfiguration siteConfiguration, 
            IApiClient apiClient)
        {
            _siteConfiguration = siteConfiguration;
            _apiClient = apiClient;
        }

        [HttpPost]
        [ActionName("verify-paypal")]
        public ActionResult VerifyPaypalAccount(VerifyPaypalAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName)
                || string.IsNullOrWhiteSpace(request.LastName)
                || string.IsNullOrWhiteSpace(request.Email))
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            GetVerifiedStatusResponse verifyResponse = null;
            try
            {
                var getVerifiedStatusRequest = new GetVerifiedStatusRequest(_apiClient.Configuration)
                                                   {
                                                       EmailAddress=request.Email,
                                                       FirstName=request.FirstName,
                                                       LastName=request.LastName
                                                   };
                verifyResponse = _apiClient.Accounts.VerifyAccount(getVerifiedStatusRequest);
                verifyResponse.Success = true;
            }
            catch (HttpChannelException exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                verifyResponse = new GetVerifiedStatusResponse(){Success = false};
            }

            return ApiResponse(new 
            {
                Success=verifyResponse.Success, 
                AccountStatus=verifyResponse.AccountStatus,
                Verified=verifyResponse.Verified
            });
        }

    }
}
