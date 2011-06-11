using System;

namespace GroupGiving.Core.Email
{
    internal class EmailTemplate
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public string FromAddress { get; set; }

        public bool IsHtml { get; set; }

        public string SenderName { get; set; }

        public string SenderAddress { get; set; }

        public string FromName { get; set; }
    }
}