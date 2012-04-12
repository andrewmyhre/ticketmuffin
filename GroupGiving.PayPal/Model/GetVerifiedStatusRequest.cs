using System.Runtime.Serialization;
using System.Xml.Serialization;
using GroupGiving.Core.Configuration;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name = "GetVerifiedStatusRequest", Namespace = "http://svcs.paypal.com/types/aa")]
    [XmlRoot(ElementName = "GetVerifiedStatusRequest", Namespace = "http://svcs.paypal.com/types/aa", IsNullable = false)]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/aa")]
    public class GetVerifiedStatusRequest : IPayPalRequest
    {
        private readonly AdaptiveAccountsConfiguration _configuration;

        public GetVerifiedStatusRequest(AdaptiveAccountsConfiguration configuration) : this()
        {
            _configuration = configuration;
            RequestEnvelope = new RequestEnvelope();
            ClientDetails = ClientDetails.FromConfiguration(configuration);
        }

        public GetVerifiedStatusRequest()
        {
            ClientDetails = ClientDetails.Default;
            RequestEnvelope = new RequestEnvelope();
            MatchCriteria = "NAME"; // only NAME is supported byPayPal at this time
        }

        [XmlElement("requestEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
        public RequestEnvelope RequestEnvelope { get; set; }

        //[XmlElement("clientDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlIgnore]
        public ClientDetails ClientDetails { get; set; }

        [XmlElement("emailAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string EmailAddress { get; set; }

        [XmlElement("firstName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
        public string FirstName { get; set; }

        [XmlElement("lastName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
        public string LastName { get; set; }

        [XmlElement("matchCriteria", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
        public string MatchCriteria { get; set; }
    }
}