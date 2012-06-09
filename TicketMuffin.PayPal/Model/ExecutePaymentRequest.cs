using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
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
}
