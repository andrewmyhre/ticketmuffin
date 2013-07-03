using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TicketMuffin.Core
{
    public class DialogueHistoryEntry
    {
        public DialogueHistoryEntry()
        {
            Timestamp = DateTime.Now;
            RequestHeaders = new Dictionary<string, string>();
        }

        public DialogueHistoryEntry(string request, string response) : this()
        {
            Request = request;
            Response = response;
        }
        public DialogueHistoryEntry(Exception exception) : this()
        {
            
        }

        public DateTime Timestamp { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }

        [XmlIgnore]
        public Dictionary<string,string> RequestHeaders { get; set; }
    }
}