using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name = "clientDetails", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "clientDetails")]
    public class ClientDetails
    {
        [DataMember(Order = 0)]
        [XmlElement(Order = 0, ElementName = "applicationId")]
        public string ApplicationId { get; set; }
        [DataMember(Order = 1)]
        [XmlElement(Order = 1, ElementName = "deviceId")]
        public string DeviceId { get; set; }
        [DataMember(Order = 2)]
        [XmlElement(Order = 2, ElementName = "ipAddress")]
        public string IpAddress { get; set; }
        [DataMember(Order = 3)]
        [XmlElement(Order = 3, ElementName = "partnerName")]
        public string PartnerName { get; set; }
    }
}