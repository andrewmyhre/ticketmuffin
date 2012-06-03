using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [System.SerializableAttribute()]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class ExecutePaymentResponse : ResponseBase
    {
        public new DialogueHistoryEntry Raw { get; set; }
    }
}