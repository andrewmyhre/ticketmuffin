using System;
using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public class PasswordResetInstructionsEmail : IEmailMessage
    {
        private readonly string _emailAddress;
        private readonly string _firstName;
        private readonly string _resetPasswordToken;

        private static EmailTemplate _template = new EmailTemplate()
        {
            Body = "Click this url or copy and paste it into your browser to be taken to a page where you can reset your password:\n\n@Model.ResetLink",
            Subject = "Reset your password - GivEnt",
            FromAddress = "noreply@givent.com",
            FromName="GivEnt",
            SenderAddress="noreply@givent.com",
            SenderName = "GiveEnt",
            IsHtml=false
        };

        public PasswordResetInstructionsEmail(string emailAddress, string firstName, string resetPasswordToken)
        {
            _emailAddress = emailAddress;
            _firstName = firstName;
            _resetPasswordToken = resetPasswordToken;
        }

        public MailMessage ToMailMessage()
        {
            MailMessage message = EmailTemplate.ToMailMessage(_template);
            message.To.Add(new MailAddress(_emailAddress, _firstName));
            message.Body += "\n\n/account/resetpassword/" + _resetPasswordToken;

            return message;
        }
    }
}