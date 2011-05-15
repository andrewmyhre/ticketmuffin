using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [CollectionDataContract(Name = "receiverList", Namespace = "http://svcs.paypal.com/types/ap")]
    [KnownType(typeof(List<Receiver>))]
    [XmlRoot(ElementName = "receiverList")]
    public class ReceiverList : List<Receiver>
    {
    }
}