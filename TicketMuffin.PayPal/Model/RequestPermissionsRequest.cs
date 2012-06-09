using System.Xml.Serialization;
using TicketMuffin.PayPal.Configuration;

namespace TicketMuffin.PayPal.Model
{
    [XmlRoot("RequestPermissions")]
    public class RequestPermissionsRequest : IPayPalRequest
    {
        private readonly AdaptiveAccountsConfiguration _paypalConfiguration;

        public RequestPermissionsRequest()
        {
            _paypalConfiguration = new AdaptiveAccountsConfiguration();
        }
        public RequestPermissionsRequest(AdaptiveAccountsConfiguration paypalConfiguration)
        {
            _paypalConfiguration = paypalConfiguration;
        }

        [XmlElement("scope")]
        public string[] Scope { get; set; }

        [XmlElement("requestEnvelope")]
        public RequestEnvelope RequestEnvelope { get; set; }

        [XmlElement("clientDetails")]
        public ClientDetails ClientDetails { get; set; }

        [XmlElement("callback")]
        public string Callback { get; set; }
    }
}