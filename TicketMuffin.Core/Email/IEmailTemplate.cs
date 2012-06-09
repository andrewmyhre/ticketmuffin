namespace TicketMuffin.Core.Email
{
    public interface IEmailTemplate
    {
        System.Net.Mail.MailMessage ToMailMessage();

    }
}