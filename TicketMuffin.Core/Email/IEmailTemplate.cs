using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public interface IEmailTemplate
    {
        System.Net.Mail.MailMessage ToMailMessage();

    }
}