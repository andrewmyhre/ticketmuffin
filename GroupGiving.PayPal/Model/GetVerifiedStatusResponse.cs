using System.Xml.Serialization;
using GroupGiving.Core.Dto;

namespace GroupGiving.PayPal.Model
{
    [System.SerializableAttribute()]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/aa")]
    [XmlRoot(ElementName="GetVerifiedStatusResponse", Namespace = "http://svcs.paypal.com/types/aa", IsNullable = false)]
    public class GetVerifiedStatusResponse : ResponseBase
    {
        [XmlElement("responseEnvelope", Order = 0, Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [XmlElement("accountStatus", Order = 1, Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AccountStatus { get; set; }

        public bool Verified { get { return AccountStatus == "VERIFIED"; } }
    }
}