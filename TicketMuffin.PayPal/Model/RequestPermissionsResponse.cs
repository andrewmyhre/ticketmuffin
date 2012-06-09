using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/perm")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/perm", IsNullable = false)]
    public class RequestPermissionsResponse : ResponseBase  
    {
        [XmlElement("responseEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [XmlElement("token", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Token { get; set; }
    }
}