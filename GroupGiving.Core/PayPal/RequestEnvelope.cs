using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GroupGiving.Core.PayPal
{
    [DataContract(Name = "requestEnvelope", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "requestEnvelope")]
    public class RequestEnvelope
    {
        [DataMember(Order = 0)]
        [XmlElement(Order = 0, ElementName = "errorLanguage")]
        public string ErrorLanguage { get; set; }
        [DataMember(Order = 1)]
        [XmlElement(Order = 1, ElementName = "detailLevel")]
        public string DetailLevel { get; set; }

    }
}