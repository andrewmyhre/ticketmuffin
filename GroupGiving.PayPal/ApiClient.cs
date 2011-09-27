using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal.Model;
using log4net;

namespace GroupGiving.PayPal
{
    public class ApiClient : IApiClient
    {
        private ILog _log = LogManager.GetLogger(typeof (ApiClient));
        private readonly ApiClientSettings _clientSettings;
        private readonly IPayPalConfiguration _payPalConfiguration;

        public ApiClient(ApiClientSettings clientSettings, IPayPalConfiguration payPalConfiguration)
        {
            _clientSettings = clientSettings;
            _payPalConfiguration = payPalConfiguration;
        }

        public PaymentGatewayResponse SendPayRequest(PayRequest request)
        {
            // set first receiver as primary
            request.Receivers.First().Primary = true;

            XmlSerializer serializer = new XmlSerializer(typeof(PayRequest));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("ns2", "http://svcs.paypal.com/types/ap");
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, request, namespaces);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(ms);
            StringBuilder xml = new StringBuilder(reader.ReadToEnd());

            _log.Debug(xml);

            HttpWebRequest oPayRequest = (HttpWebRequest)WebRequest.Create(_clientSettings.ApiEndpoint);
            oPayRequest.Method = "POST";
            byte[] array = Encoding.UTF8.GetBytes(xml.ToString());
            oPayRequest.ContentLength = array.Length;
            oPayRequest.ContentType = "text/xml;charset=utf-8";
            // set the HTTP Headers
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-USERID", _clientSettings.Username);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", _clientSettings.Password);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", _clientSettings.Signature);
            oPayRequest.Headers.Add("X-PAYPAL-SERVICE-VERSION", _clientSettings.ApiVersion);
            oPayRequest.Headers.Add("X-PAYPAL-APPLICATION-ID", request.ClientDetails.ApplicationId);
            oPayRequest.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", _clientSettings.RequestDataBinding);
            oPayRequest.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", _clientSettings.ResponseDataBinding);
            // send the request
            Stream oStream = oPayRequest.GetRequestStream();
            oStream.Write(array, 0, array.Length);
            oStream.Close();
            // get the response
            HttpWebResponse oPayResponse = (HttpWebResponse)oPayRequest.GetResponse();
            StreamReader sreader = new StreamReader(oPayResponse.GetResponseStream());
            var responseString = sreader.ReadToEnd();
            sreader.Close();

            _log.Debug(responseString);

            var doc = XDocument.Parse(responseString);

            if (doc.Root.Name.LocalName=="FaultMessage")
            {
                return new PaymentGatewayResponse()
                {
                    ResponseEnvelope = new ResponseEnvelope()
                    {
                        Ack = doc.Root.Element("responseEnvelope").Element("ack").Value,
                        Build = long.Parse(doc.Root.Element("responseEnvelope").Element("build").Value),
                        CorrelationId = doc.Root.Element("responseEnvelope").Element("correlationId").Value,
                        Timestamp = DateTime.Parse(doc.Root.Element("responseEnvelope").Element("timestamp").Value)
                    },
                    Errors = (from e in doc.Root.Elements("error")
                                  select new ResponseError()
                                    {
                                        ErrorId = e.Element("errorId") != null ? e.Element("errorId").Value : "",
                                        Domain = e.Element("domain") != null ? e.Element("domain").Value : "",
                                        SubDomain = e.Element("subdomain") != null ? e.Element("subdomain").Value : "",
                                        Severity = e.Element("severity") != null ? e.Element("severity").Value : "",
                                        Category = e.Element("category") != null ? e.Element("category").Value : "",
                                        Message = e.Element("message") != null ? e.Element("message").Value : "",
                                        Parameter=e.Element("parameter") != null ? e.Element("parameter").Value : ""
                                    })  
                };
            }

            var responseObject = new PaymentGatewayResponse()
            {
                TransactionId = doc.Root.Element("payKey").Value,
                PaymentExecStatus = doc.Root.Element("paymentExecStatus").Value,
                ResponseEnvelope = new ResponseEnvelope()
                {
                    Ack = doc.Root.Element("responseEnvelope").Element("ack").Value,
                    Build =long.Parse(doc.Root.Element("responseEnvelope").Element("build").Value),
                    CorrelationId =doc.Root.Element("responseEnvelope").Element("correlationId").Value,
                    Timestamp =DateTime.Parse(doc.Root.Element("responseEnvelope").Element("timestamp").Value)
                },
                PaymentPageUrl = _payPalConfiguration.PayFlowProPaymentPage
            };

            return responseObject;
        }
    }
}