using System.Collections.Generic;
using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public class ThanksForRegisteringEmail : IEmailMessage
    {
        private readonly string _toAddress;
        private readonly string _toName;

        private static EmailTemplate _template = new EmailTemplate()
        {
            Subject = "Thanks for registering @Model.FirstName",
            Body = "Hi @Model.FirstName\n\nWe're glad you've taken the time to register on our site.",
            FromAddress = "noreply@givent.com",
            FromName = "GivEnt",
            SenderAddress = "noreply@givent.com",
            SenderName = "GivEnt",
            IsHtml=false

        };

        public ThanksForRegisteringEmail(string toAddress, string toName)
        {
            _toAddress = toAddress;
            _toName = toName;
        }

        public MailMessage ToMailMessage()
        {
            MailMessage message = new MailMessage();
            message.Subject = _template.Subject;
            message.Body = _template.Body;
            message.From = new MailAddress(_template.FromAddress, _template.FromName);
            message.Sender = new MailAddress(_template.SenderAddress, _template.SenderName);
            message.To.Add(new MailAddress(_toAddress, _toName));
            message.IsBodyHtml = false;
            message.From = new MailAddress(_template.FromAddress, _template.FromName);

            return message;
        }
    }
}