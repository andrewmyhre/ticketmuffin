using System.Runtime.Serialization;
using System.Xml.Serialization;
using GroupGiving.Core.Configuration;

namespace GroupGiving.Core.PayPal
{
    [DataContract(Name = "PayRequest", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "PayRequest")]
    public class PayRequest : IPayPalRequest
    {
        public PayRequest(AdaptiveAccountsConfiguration adaptiveAccounts) : this()
        {
            CancelUrl = adaptiveAccounts.FailureCallbackUrl;
            ReturnUrl = adaptiveAccounts.SuccessCallbackUrl;
            ClientDetails = ClientDetails.FromConfiguration(adaptiveAccounts);
        }
        public PayRequest()
        {
            ClientDetails = ClientDetails.Default;

            Receivers = new Receiver[0];
            RequestEnvelope = new RequestEnvelope();
            ActionType = "PAY";

        }

        [DataMember(Order = 0)]
        [XmlElement(ElementName="requestEnvelope", Order=0)]
        public RequestEnvelope RequestEnvelope { get; set; }
        [DataMember(Order = 1)]
        [XmlElement(Order = 1, ElementName="clientDetails")]
        public ClientDetails ClientDetails { get; set; }
        [DataMember(Order = 2)]
        [XmlElement(Order = 2, ElementName="actionType")]
        public string ActionType { get; set; }
        [DataMember(Order = 3)]
        [XmlElement(Order = 3, ElementName="cancelUrl")]
        public string CancelUrl { get; set; }
        [DataMember(Order = 4)]
        [XmlElement(Order = 4, ElementName = "returnUrl")]
        public string ReturnUrl { get; set; }
        [DataMember(Order = 5)]
        [XmlElement(Order = 5, ElementName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [DataMember(Order = 6)]
        [XmlElement(Order = 6, ElementName = "feesPayer")]
        public string FeesPayer { get; set; }
        [DataMember(Order = 7)]
        [XmlElement(Order = 7, ElementName = "memo")]
        public string Memo { get; set; }
        [DataMember(Order = 8)]
        [XmlArray(ElementName="receiverList", Order = 8)]
        [XmlArrayItem(ElementName = "receiver")]
        public Receiver[] Receivers { get; set; }
    }
}