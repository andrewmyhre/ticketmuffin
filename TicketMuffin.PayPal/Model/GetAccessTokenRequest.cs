using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [XmlRoot("GetAccessTokenRequest")]
    public class GetAccessTokenRequest : IPayPalRequest
    {
        [XmlElement("responseEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RequestEnvelope RequestEnvelope { get; set; }

        public ClientDetails ClientDetails { get; set; }

        [XmlElement("token")]
        public string Token { get; set; }

        [XmlElement("verifier")]
        public string Verifier { get; set; }
    }
}