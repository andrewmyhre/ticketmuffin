﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name = "PayRequest", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "PayRequest")]
    public class PayRequest
    {
        public PayRequest()
        {
            ClientDetails = new ClientDetails()
            {
                ApplicationId = "APP-80W284485P519543T",
                DeviceId = "255.255.255.255",
                IpAddress = "255.255.255.255",
                PartnerName = "MyCompanyName"

            };
            Receivers = new List<Receiver>();
            RequestEnvelope = new RequestEnvelope();
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
        public List<Receiver> Receivers { get; set; }
    }
}