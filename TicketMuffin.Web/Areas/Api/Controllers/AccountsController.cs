using System.Net;
using System.Web.Mvc;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Models;
using log4net;

namespace TicketMuffin.Web.Areas.Api.Controllers
{
    public class AccountsController : ApiControllerBase
    {
        private ILog logger = LogManager.GetLogger(typeof (AccountsController));
        private readonly IApiClient _apiClient;
        //
        // GET: /Api/Accounts/
        public AccountsController(IApiClient apiClient)
        {
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
                logger.Error(exception);
                logger.Error(exception.FaultMessage.Raw.Request);
                logger.Error(exception.FaultMessage.Raw.Response);
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
