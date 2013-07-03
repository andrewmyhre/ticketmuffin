using System.Xml.Serialization;
using TicketMuffin.Core;

namespace TicketMuffin.PayPal.Model
{
    [System.SerializableAttribute()]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class ExecutePaymentResponse : ResponseBase
    {
        public new DialogueHistoryEntry Raw { get; set; }
    }
}