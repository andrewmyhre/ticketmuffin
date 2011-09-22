using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.PayPal.Configuration;
using GroupGiving.Web.Areas.Api.Models;
using GroupGiving.Web.Code;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class AccountsController : ApiControllerBase
    {
        //
        // GET: /Api/Accounts/
        public AccountsController(IRepository<GroupGivingEvent> eventRepository) : base(eventRepository)
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

    [DataContract(Name="verifyPaypalAccountRequest", Namespace=Code.Api.Namespace)]
    public class VerifyPaypalAccountRequest
    {
        [DataMember(Name = "firstName", EmitDefaultValue = false)]
        [Required]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName", EmitDefaultValue = false)]
        [Required]
        public string LastName { get; set; }

        [DataMember(Name = "email", EmitDefaultValue = false)]
        [Required]
        public string Email { get; set; }
    }

    [DataContract(Name="paypalAccountVerification", Namespace=Api.Code.Api.Namespace)]
    public class VerifyPaypalAccountResponse
    {
        [DataMember(Name="success", EmitDefaultValue = true)]
        public bool Success { get; set; }

        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public ErrorResponse Errors { get; set; }

        [DataMember(Name="status")]
        public string AccountStatus { get; set; }
    }
}
