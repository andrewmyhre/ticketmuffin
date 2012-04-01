using System;
using System.Collections.Generic;

namespace GroupGiving.Core.Dto
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

        public DateTime Timestamp { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }

        public Dictionary<string,string> RequestHeaders { get; set; }
    }
}