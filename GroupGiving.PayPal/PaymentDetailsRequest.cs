using System.Xml.Serialization;
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
            ClientDetails = new ClientDetails()
            {
                ApplicationId = "APP-80W284485P519543T",
                DeviceId = "255.255.255.255",
                IpAddress = "255.255.255.255",
                PartnerName = "MyCompanyName"

            };
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