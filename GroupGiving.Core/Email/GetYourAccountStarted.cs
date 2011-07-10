using System;
using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public class GetYourAccountStarted : IEmailMessage
    {
        private readonly string _emailAddress;
        private readonly string _resetPasswordToken;

        public GetYourAccountStarted(string emailAddress, string resetPasswordToken)
        {
            _emailAddress = emailAddress;
            _resetPasswordToken = resetPasswordToken;
        }

        private static EmailTemplate _template = new EmailTemplate()
        {
            Body = "Click this url or copy and paste it into your browser to be taken to a page where you can set your password:",
            Subject = "TicketMuffin - let's get your password set up",
            FromAddress = "noreply@ticketmuffin.com",
            FromName = "TicketMuffin",
            SenderAddress = "noreply@ticketmuffin.com",
            SenderName = "TicketMuffin",
            IsHtml = false
        };

        public MailMessage ToMailMessage()
        {
            MailMessage message = EmailTemplate.ToMailMessage(_template);
            message.To.Add(new MailAddress(_emailAddress));
            message.Body += "\n\n/account/getstarted/" + _resetPasswordToken;

            return message;
        }
    }
}