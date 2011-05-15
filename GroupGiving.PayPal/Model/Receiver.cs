using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name = "receiver", Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(ElementName = "receiver")]
    public class Receiver
    {
        [DataMember(Order = 0)]
        [XmlElement(Order = 0, ElementName="amount")]
        public string Amount { get; set; }
        [DataMember(Order = 1)]
        [XmlElement(Order = 1, ElementName = "email")]
        public string Email
        {
            get;
            set; 
        }

        [XmlElement(Order = 2, ElementName = "primary")]
        public bool Primary { get; set; }

        public Receiver(string amount, string email)
        {
            Amount = amount;
            Email = email;
        }

        public Receiver()
        {
            
        }
    }
}