using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    public class ExecutePaymentRequest : IPayPalRequest
    {
        public ExecutePaymentRequest()
        {
            
        }

        public ExecutePaymentRequest(string payKey)
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

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class ExecutePaymentResponse
    {
        
    }
}
