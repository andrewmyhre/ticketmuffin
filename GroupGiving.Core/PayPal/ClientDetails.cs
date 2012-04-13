using System.Runtime.Serialization;
using System.Xml.Serialization;
using GroupGiving.Core.Configuration;

namespace GroupGiving.Core.PayPal
{
    [DataContract(Name = "clientDetails", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "clientDetails")]
    public class ClientDetails
    {
        private ClientDetails()
        {
            
        }
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

        public static ClientDetails Default
        {
            get
            {
                return new ClientDetails()
                           {
                               ApplicationId = "APP-80W284485P519543T",
                               DeviceId = "255.255.255.255",
                               IpAddress = "255.255.255.255",
                               PartnerName = "TicketMuffin"

                           };
            }
        }

        public static ClientDetails FromConfiguration(AdaptiveAccountsConfiguration configuration)
        {
            var client = ClientDetails.Default;
            client.ApplicationId = configuration.ApplicationId;
            return client;
        }
    }
}