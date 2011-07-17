using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using NUnit.Framework;

namespace PayPal.Tests.Integration
{
    [TestFixture]
    public class ChainedPayments
    {
        [Test]
        public void paymentRequestSerialisesCorrectly()
        {
            ApiClient client = new ApiClient(new ApiClientSettings()
                                                 {
                                                     Username = "seller_1304843436_biz_api1.gmail.com",
                                                     Password = "1304843443",
                                                     Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7"
                                                 },
                                                 new PayPalConfiguration());

            PayRequest request = new PayRequest();
            request.ActionType = "PAY";
            request.CancelUrl = "https://MyCancelURL";
            request.ReturnUrl = "https://MyReturnURL";
            request.ClientDetails = new ClientDetails()
            {
                ApplicationId = "APP-80W284485P519543T",
                DeviceId = "255.255.255.255",
                IpAddress = "255.255.255.255",
                PartnerName = "MyCompanyName"
            };
            request.CurrencyCode = "USD";
            request.FeesPayer = "EACHRECEIVER";
            request.Memo = "testing my first pay call";
            request.Receivers = new ReceiverList();
            request.Receivers.Add(new Receiver("100", "seller_1304843436_biz@gmail.com"));
            request.Receivers.Add(new Receiver("90", "sellr2_1304843519_biz@gmail.com"));
            request.RequestEnvelope = new RequestEnvelope() { DetailLevel = "ReturnAll", ErrorLanguage = "en_US" };

            var response = client.SendPayRequest(request);

            // leave the console up for us to physically read the response data
            Assert.That(response.TransactionId, Is.StringStarting("AP-"));
            Assert.That(response.ResponseEnvelope.Ack, Is.StringMatching("Success"));
            Assert.That(response.PaymentExecStatus, Is.StringMatching("CREATED"));
            Console.WriteLine("Paykey: {0}", response.TransactionId);
            
        }

        [TestCase("seller_1304843436_biz_api1.gmail.com", "1304843443", "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7")]
        [Ignore("brittle")]
        public void CanSubmitNameValueRequest(string username, string password, string signature)
        {
            // API Credentials - supply your own sandbox credentials
            string sAPIUser = username;
            string sAPIPassword = password;
            string sAPISignature = signature;

            // API endpoint for the Refund call in the Sandbox
            string sAPIEndpoint = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";

            // Version that you are coding against
            string sVersion = "1.1.0";

            // Error Langugage
            string sErrorLangugage = "en_US";

            // Detail Level
            string sDetailLevel = "ReturnAll";

            // Request Data Binding
            string sRequestDataBinding = "XML";

            // Response Data Binding
            string sResponseDataBinding = "XML";

            // Application ID
            string sAppID = "APP-80W284485P519543T";

            // other clientDetails fields
            string sIpAddress = "255.255.255.255";
            string sPartnerName = "MyCompanyName";
            string sDeviceID = "255.255.255.255";

            // Currency Code
            string sCurrencyCode = "USD";

            // Action Type
            string sActionType = "PAY";

            // ReturnURL and CancelURL used for approval flow
            string sReturnURL = "https://MyReturnURL";
            string sCancelURL = "https://MyCancelURL";

            // who pays the fees
            string sFeesPayer = "EACHRECEIVER";

            // memo field
            string sMemo = "testing my first pay call";

            // transaction amount
            string sAmount = "5";

            // supply your own sandbox accounts for receiver and sender
            string sReceiverEmail = "sellr2_1304843519_biz@gmail.com";
            string sSenderEmail = "buyer1_1304843364_per@gmail.com";

            string sTrackingID = System.Guid.NewGuid().ToString();

            // construct the XML request string
            StringBuilder sRequest = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sRequest.Append("<PayRequest xmlns:ns2=\"http://svcs.paypal.com/types/ap\">");
            // requestEnvelope fields
            sRequest.Append("<requestEnvelope><errorLanguage>");
            sRequest.Append(sErrorLangugage);
            sRequest.Append("</errorLanguage><detailLevel>");
            sRequest.Append(sDetailLevel);
            sRequest.Append("</detailLevel></requestEnvelope>");
            // clientDetails fields
            sRequest.Append("<clientDetails><applicationId>");
            sRequest.Append(sAppID);
            sRequest.Append("</applicationId><deviceId>");
            sRequest.Append(sDeviceID);
            sRequest.Append("</deviceId><ipAddress>");
            sRequest.Append(sIpAddress);
            sRequest.Append("</ipAddress><partnerName>");
            sRequest.Append(sPartnerName);
            sRequest.Append("</partnerName></clientDetails>");
            // request specific data fields
            sRequest.Append("<actionType>");
            sRequest.Append(sActionType);
            sRequest.Append("</actionType><cancelUrl>");
            sRequest.Append(sCancelURL);
            sRequest.Append("</cancelUrl><returnUrl>");
            sRequest.Append(sReturnURL);
            sRequest.Append("</returnUrl><currencyCode>");
            sRequest.Append(sCurrencyCode);
            sRequest.Append("</currencyCode><feesPayer>");
            sRequest.Append(sFeesPayer);
            sRequest.Append("</feesPayer><memo>");
            sRequest.Append(sMemo);
            sRequest.Append("</memo><receiverList><receiver><amount>");
            sRequest.Append(sAmount);
            sRequest.Append("</amount><email>");
            sRequest.Append(sReceiverEmail);
            sRequest.Append("</email></receiver></receiverList><senderEmail>");
            sRequest.Append(sSenderEmail);
            sRequest.Append("</senderEmail><trackingId>");
            sRequest.Append(sTrackingID);
            sRequest.Append("</trackingId></PayRequest>");
            Console.WriteLine(sRequest);
            Console.WriteLine();

            // get ready to make the call
            string sResponse = SendPayRequest(sRequest, sAppID, sAPIEndpoint, sAPIUser, sAPIPassword, sAPISignature, sVersion, sRequestDataBinding, sResponseDataBinding);

            // write the response string to the console
            System.Console.WriteLine(sResponse);

            // leave the console up for us to physically read the response data
            Console.Read();

            Assert.That(sResponse, Is.Not.StringContaining("Failure"));

            XDocument doc = XDocument.Parse(sResponse);
            Assert.That(doc.Root.Element("payKey").Value, Is.StringStarting("AP-"));
            Assert.That(doc.Root.Element("responseEnvelope").Element("ack").Value, Is.StringMatching("Success"));
            Assert.That(doc.Root.Element("paymentExecStatus").Value, Is.StringMatching("CREATED"));
            Console.WriteLine("Paykey: {0}", doc.Root.Element("payKey").Value);
        }

        private string SendPayRequest(StringBuilder sRequest, string sAppID, string sAPIEndpoint, string sAPIUser, string sAPIPassword, string sAPISignature, string sVersion, string sRequestDataBinding, string sResponseDataBinding)
        {
            HttpWebRequest oPayRequest = (HttpWebRequest)WebRequest.Create(sAPIEndpoint);
            oPayRequest.Method = "POST";
            byte[] array = Encoding.UTF8.GetBytes(sRequest.ToString());
            oPayRequest.ContentLength = array.Length;
            oPayRequest.ContentType = "text/xml;charset=utf-8";
            // set the HTTP Headers
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-USERID", sAPIUser);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", sAPIPassword);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", sAPISignature);
            oPayRequest.Headers.Add("X-PAYPAL-SERVICE-VERSION", sVersion);
            oPayRequest.Headers.Add("X-PAYPAL-APPLICATION-ID", sAppID);
            oPayRequest.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", sRequestDataBinding);
            oPayRequest.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", sResponseDataBinding);
            // send the request
            Stream oStream = oPayRequest.GetRequestStream();
            oStream.Write(array, 0, array.Length);
            oStream.Close();
            // get the response
            HttpWebResponse oPayResponse = (HttpWebResponse)oPayRequest.GetResponse();
            StreamReader oStreamReader = new StreamReader(oPayResponse.GetResponseStream());
            string sResponse = oStreamReader.ReadToEnd();
            oStreamReader.Close();
            return sResponse;
        }
    }
}
