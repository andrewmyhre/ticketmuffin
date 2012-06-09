using System;
using System.Net.Mail;

namespace TicketMuffin.Core.Email
{
    public class SimpleSmtpEmailRelayService : IEmailRelayService
    {
        public void SendEmail(IEmailTemplate emailTemplate)
        {
            SmtpClient smtp = new SmtpClient();

            try
            {
                smtp.Send(emailTemplate.ToMailMessage());
            } catch (Exception ex)
            {
                // TODO log error
            } finally
            {
                
            }
        }
    }
}