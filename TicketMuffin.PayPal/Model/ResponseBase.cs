using System;
using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [Serializable]
    public class ResponseBase
    {
        [XmlIgnore]
        public DialogueHistoryEntry Raw { get; set; }
    }
}