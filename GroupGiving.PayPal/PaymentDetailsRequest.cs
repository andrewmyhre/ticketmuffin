using System.Xml.Serialization;
using GroupGiving.Core.PayPal;
using GroupGiving.PayPal.Model;

namespace GroupGiving.PayPal
{
    public class PaymentDetailsRequest : IPayPalRequest
    {
        public PaymentDetailsRequest()
        {
        }


        public PaymentDetailsRequest(string payKey)
        {
            PayKey = payKey;
            ClientDetails = ClientDetails.Default;
            RequestEnvelope = new RequestEnvelope()
                                  {
                                      ErrorLanguage = "en_US"
                                  };

        }

        [XmlElement(ElementName = "payKey")]
        public string PayKey { get; set; }

        [XmlElement(ElementName = "requestEnvelope")]
        public RequestEnvelope RequestEnvelope { get; set; }

        [XmlElement(ElementName = "clientDetails")]
        public ClientDetails ClientDetails { get; set; }
    }
}