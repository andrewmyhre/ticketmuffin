using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GroupGiving.Core.Dto;

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

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class ExecutePaymentResponse : ResponseBase
    {
        public DialogueHistoryEntry Raw { get; set; }


    }
}
