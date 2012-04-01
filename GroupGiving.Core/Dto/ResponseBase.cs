using System.Xml.Serialization;

namespace GroupGiving.Core.Dto
{
    public class ResponseBase
    {
        [XmlIgnore]
        public DialogueHistoryEntry Raw { get; set; }
    }
}