using System;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [Serializable]
    public class ResponseBase
    {
        [XmlIgnore]
        public DialogueHistoryEntry Raw { get; set; }
    }
}