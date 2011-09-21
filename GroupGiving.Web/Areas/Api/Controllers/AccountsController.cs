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
using GroupGiving.PayPal.AdaptiveAccounts;
using GroupGiving.PayPal.Configuration;
using GroupGiving.Web.Areas.Api.Models;
using PayPal.Platform.SDK;
using PayPal.Services.Private.AA;
using AdaptiveAccounts = PayPal.Platform.SDK.AdaptiveAccounts;

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

            BaseAPIProfile profile = BaseApiProfileFactory.CreateFromConfiguration(configuration);

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
                    response.Verified = false;
                    return ApiResponse(response);
                } else if (verifyResponse.responseEnvelope.ack != AckCode.Success)
                {
                    response.Success = false;
                    response.Verified = false;
                    return ApiResponse(response);
                }

                response.Success = true;
                response.Verified = verifyResponse.accountStatus == "VERIFIED";
                return ApiResponse(response);
            }
            catch
            {
                response.Success = false;
                response.Verified = false;
                return ApiResponse(response);
            }
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

        [DataMember(Name="verified", EmitDefaultValue = true)]
        public bool Verified { get; set; }
    }
}
