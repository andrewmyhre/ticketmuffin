using System.Xml.Serialization;
using GroupGiving.Core.Dto;

namespace GroupGiving.PayPal.Model
{
    public class RefundRequest : IPayPalRequest
    {
        [XmlElement(ElementName = "payKey")]
        public string PayKey { get; set; }

        [XmlElement(ElementName = "currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement(ElementName = "requestEnvelope")]
        public RequestEnvelope RequestEnvelope { get; set; }

        [XmlElement(ElementName = "clientDetails")]
        public ClientDetails ClientDetails { get; set; }

        public ReceiverList Receivers { get; set; }

        public RefundRequest()
        {
            
        }

        public RefundRequest(string payKey)
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
    }

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class RefundResponse : ResponseBase
    {
        [XmlElement(ElementName = "responseEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        public DialogueHistoryEntry Raw { get; set; }
    }
}