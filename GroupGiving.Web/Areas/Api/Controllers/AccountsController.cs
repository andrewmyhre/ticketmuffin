using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class AccountsController : ApiControllerBase
    {
        //
        // GET: /Api/Accounts/
        public AccountsController(IRepository<GroupGivingEvent> eventRepository)
        {
        }

        [HttpPost]
        [ActionName("verify-paypal")]
        public ActionResult VerifyPaypalAccount(VerifyPaypalAccountRequest request)
        {
            var configuration =
    ConfigurationManager.GetSection("adaptiveAccounts") as PaypalAdaptiveAccountsConfigurationSection;

            return ApiResponse(new PaypalAccountService(configuration).VerifyPaypalAccount(request));
        }

    }
}
