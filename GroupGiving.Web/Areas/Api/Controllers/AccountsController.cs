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
using GroupGiving.Core.PayPal;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class AccountsController : ApiControllerBase
    {
        private readonly ISiteConfiguration _siteConfiguration;
        private readonly IAdaptiveAccountsService _adaptiveAccountsService;
        //
        // GET: /Api/Accounts/
        public AccountsController(IRepository<GroupGivingEvent> eventRepository, ISiteConfiguration siteConfiguration, IAdaptiveAccountsService adaptiveAccountsService)
        {
            _siteConfiguration = siteConfiguration;
            _adaptiveAccountsService = adaptiveAccountsService;
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
                verifyResponse = _adaptiveAccountsService.AccountIsVerified(request.Email, request.FirstName,
                                                                            request.LastName);
            } catch (HttpChannelException exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                verifyResponse = new GetVerifiedStatusResponse(){Success = false};
            }

            return ApiResponse(verifyResponse);
        }

    }
}
