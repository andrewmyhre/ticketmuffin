using System;

namespace GroupGiving.Core.Dto
{
    public class DialogueHistoryEntry
    {
        public DialogueHistoryEntry()
        {
            Timestamp = DateTime.Now;
        }

        public DialogueHistoryEntry(string request, string response) : this()
        {
            Request = request;
            Response = response;
        }

        public DateTime Timestamp { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}