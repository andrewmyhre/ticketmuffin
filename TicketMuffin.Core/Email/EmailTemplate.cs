using System;
using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public class EmailTemplate
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public string FromAddress { get; set; }

        public bool IsHtml { get; set; }

        public string SenderName { get; set; }

        public string SenderAddress { get; set; }

        public string FromName { get; set; }

        public static MailMessage ToMailMessage(EmailTemplate template)
        {
            MailMessage message = new MailMessage();
            message.Subject = template.Subject;
            message.Body = template.Body;
            message.From = new MailAddress(template.FromAddress, template.FromName);
            message.Sender = new MailAddress(template.SenderAddress, template.SenderName);
            message.IsBodyHtml = false;
            message.From = new MailAddress(template.FromAddress, template.FromName);
            return message;
        }

    }
}