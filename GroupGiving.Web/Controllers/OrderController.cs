using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/

        public ActionResult Index(int? eventId)
        {
            return View();
        }

        public ActionResult PaymentRequest()
        {
            return View();
        }

        public ActionResult StartRequest()
        {
            PayResponse response = SendPaymentRequest();

            var viewModel = new OrderRequestViewModel();
            viewModel.PayPalPostUrl = ConfigurationManager.AppSettings["PayFlowProPaymentPage"];
            viewModel.Ack = response.ResponseEnvelope.Ack;
            viewModel.PayKey = response.PayKey;
            viewModel.Errors = response.Errors;

            if (response.Errors != null && response.Errors.Count() > 0)
                return View(viewModel);

            return Redirect(string.Format(ConfigurationManager.AppSettings["PayFlowProPaymentPage"], response.PayKey));
        }

        private PayResponse SendPaymentRequest()
        {
            IApiClient payPal = new ApiClient(new ApiClientSettings()
                                                  {
                                                      Username = "seller_1304843436_biz_api1.gmail.com",
                                                      Password = "1304843443",
                                                      Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7"
                                                  });
            var request = new PayRequest();
            request.ActionType = "PAY";
            request.CurrencyCode = "GBP";
            request.FeesPayer = "EACHRECEIVER";
            request.Memo = "test order";
            request.CancelUrl = "http://" + Request.Url.Authority + "/Order/Cancel?payKey=${payKey}.";
            request.ReturnUrl = "http://" + Request.Url.Authority + "/Order/Return?payKey=${payKey}.";
            request.Receivers.Add(new Receiver("10", "seller_1304843436_biz@gmail.com"));
            request.Receivers.Add(new Receiver("9", "sellr2_1304843519_biz@gmail.com"));
            return payPal.SendPayRequest(request);
        }

        public ActionResult Return()
        {
            return View();
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
