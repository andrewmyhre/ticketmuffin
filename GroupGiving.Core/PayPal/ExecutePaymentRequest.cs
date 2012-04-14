using System.Xml.Serialization;
using GroupGiving.Core.Dto;

namespace GroupGiving.Core.PayPal
{
    public class ExecutePaymentRequest : IPayPalRequest
    {
        public ExecutePaymentRequest()
        {
            ClientDetails = ClientDetails.Default;
            RequestEnvelope = new RequestEnvelope();
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
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class ExecutePaymentResponse : ResponseBase
    {
        public DialogueHistoryEntry Raw { get; set; }


    }
}
