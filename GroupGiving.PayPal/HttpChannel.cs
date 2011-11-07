using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using GroupGiving.Core;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class HttpChannel
    {
        public TResponse ExecuteRequest<TRequest, TResponse>(string action, TRequest request, ApiClientSettings clientSettings) 
            where TRequest : IPayPalRequest
        {
            // serialise the request
            XmlSerializer serializer = new XmlSerializer(typeof(TRequest));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("ns2", "http://svcs.paypal.com/types/ap");
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, request, namespaces);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(ms);
            StringBuilder xml = new StringBuilder(reader.ReadToEnd());

            // create the http request and add headers
            HttpWebRequest oPayRequest = (HttpWebRequest)WebRequest.Create(clientSettings.ActionUrl(action));
            oPayRequest.Method = "POST";
            byte[] array = Encoding.UTF8.GetBytes(xml.ToString());
            oPayRequest.ContentLength = array.Length;
            oPayRequest.ContentType = "text/xml;charset=utf-8";
            // set the HTTP Headers
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-USERID", clientSettings.Username);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", clientSettings.Password);
            oPayRequest.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", clientSettings.Signature);
            oPayRequest.Headers.Add("X-PAYPAL-SERVICE-VERSION", clientSettings.ApiVersion);
            oPayRequest.Headers.Add("X-PAYPAL-APPLICATION-ID", request.ClientDetails.ApplicationId);
            oPayRequest.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", clientSettings.RequestDataBinding);
            oPayRequest.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", clientSettings.ResponseDataBinding);

            // send the request
            Stream oStream = oPayRequest.GetRequestStream();
            oStream.Write(array, 0, array.Length);
            oStream.Close();

            // get the response
            HttpWebResponse oPayResponse = (HttpWebResponse)oPayRequest.GetResponse();
            StreamReader sreader = new StreamReader(oPayResponse.GetResponseStream());
            var responseString = sreader.ReadToEnd();
            sreader.Close();

            // check for fault message
            if (ResponseIsFaultMessage(responseString))
            {
                var faultMessage = DeserializeObject<FaultMessage>(responseString);
                System.Diagnostics.Debug.WriteLine("fault message:");
                System.Diagnostics.Debug.WriteLine("errorId: " + faultMessage.Error.ErrorId);
                System.Diagnostics.Debug.WriteLine("domain: " + faultMessage.Error.Domain);
                System.Diagnostics.Debug.WriteLine("severity: " + faultMessage.Error.Severity);
                System.Diagnostics.Debug.WriteLine("category: " + faultMessage.Error.Category);
                System.Diagnostics.Debug.WriteLine("parameter: " + faultMessage.Error.Parameter);
                System.Diagnostics.Debug.WriteLine("message: " + faultMessage.Error.Message);
                throw new HttpChannelException(faultMessage);
            }

            // deserialise the response
            System.Diagnostics.Debug.WriteLine("response from paypal:");
            System.Diagnostics.Debug.WriteLine(responseString);
            XmlSerializer deserializer = new XmlSerializer(typeof(TResponse));
            byte[] data = ASCIIEncoding.ASCII.GetBytes(responseString);
            MemoryStream responseStream = new MemoryStream(data);
            TResponse responseObject = (TResponse)deserializer.Deserialize(responseStream);

            return responseObject;

        }

        private static T DeserializeObject<T>(string response)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T), "ns");
            byte[] data = ASCIIEncoding.ASCII.GetBytes(response);
            MemoryStream responseStream = new MemoryStream(data);
            var responseObject = (T)deserializer.Deserialize(responseStream);
            return responseObject;
        }

        private bool ResponseIsFaultMessage(string response)
        {
            var doc = XDocument.Parse(response);
            return doc.Root.Name.LocalName == "FaultMessage";
        }
        private FaultMessage ParseFaultMessage(string response)
        {
            var doc = XDocument.Parse(response);
            return new FaultMessage()
                       {
                           responseEnvelope = new PayResponseResponseEnvelope()
                                                  {
                                                      ack = doc.Root.Element("responseEnvelope").Element("ack").Value,
                                                      build = doc.Root.Element("responseEnvelope").Element("build").Value,
                                                      correlationId = doc.Root.Element("responseEnvelope").Element("correlationId").Value,
                                                      timestamp = doc.Root.Element("responseEnvelope").Element("timestamp").Value
                                                  },
                           Error = 
                               new PayPalError()
                                   {
                                       ErrorId = doc.Root.Element("error").Element("errorId") != null ? doc.Root.Element("error").Element("errorId").Value : "",
                                       Domain = doc.Root.Element("error").Element("domain") != null ? doc.Root.Element("error").Element("domain").Value : "",
                                       SubDomain = doc.Root.Element("error").Element("subdomain") != null ? doc.Root.Element("error").Element("subdomain").Value : "",
                                       Severity = doc.Root.Element("error").Element("severity") != null ? doc.Root.Element("error").Element("severity").Value : "",
                                       Category = doc.Root.Element("error").Element("category") != null ? doc.Root.Element("error").Element("category").Value : "",
                                       Message = doc.Root.Element("error").Element("message") != null ? doc.Root.Element("error").Element("message").Value : "",
                                       Parameter = doc.Root.Element("error").Element("parameter") != null ? doc.Root.Element("error").Element("parameter").Value : ""
                                   }
                       };
        }

    }
}