using System.Runtime.Serialization;
using System.Xml.Serialization;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;

namespace GroupGiving.Core.PayPal
{
    [System.SerializableAttribute()]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/aa")]
    [XmlRoot(ElementName="GetVerifiedStatusResponse", Namespace = "http://svcs.paypal.com/types/aa", IsNullable = false)]
    [DataContract(Name="GetVerifiedStatusResponse")]
    public class GetVerifiedStatusResponse : ResponseBase
    {
        [XmlElement("responseEnvelope", Order = 0, Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [DataMember(Name="responseEnvelope")]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [XmlElement("accountStatus", Order = 1, Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [DataMember(Name="accountStatus")]
        public string AccountStatus { get; set; }

        [DataMember(Name="verified", Order=2)]
        [XmlElement("verified", Order=2)]
        public bool Verified { get { return AccountStatus == "VERIFIED"; } set {} }

        [DataMember(Name="success", Order=3)]
        [XmlElement("success", Order=3)]
        public bool Success { get; set; }
    }
}