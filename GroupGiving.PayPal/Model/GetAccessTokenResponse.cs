using System.Xml.Schema;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/perm")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/perm", IsNullable = false)]
    public class GetAccessTokenResponse : ResponseBase
    {
        [XmlElement("responseEnvelope", Form = XmlSchemaForm.Unqualified)]
        private PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [XmlElement("scope",Form=XmlSchemaForm.Unqualified)]
        public string[] Scope { get; set; }

        [XmlElement("token", Form = XmlSchemaForm.Unqualified)]
        public string Token { get; set; }

        [XmlElement("tokenSecret", Form = XmlSchemaForm.Unqualified)]
        public string TokenSecret { get; set; }
    }
}