using System;
using System.Xml.Serialization;

namespace GroupGiving.Core.Dto
{
    [Serializable]
    public class ResponseBase
    {
        [XmlIgnore]
        public DialogueHistoryEntry Raw { get; set; }
    }
}