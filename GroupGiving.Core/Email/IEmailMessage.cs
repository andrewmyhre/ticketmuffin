using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public interface IEmailMessage
    {
        System.Net.Mail.MailMessage ToMailMessage();

    }
}