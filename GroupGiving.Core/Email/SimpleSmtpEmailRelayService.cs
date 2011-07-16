using System;
using System.Net.Mail;

namespace GroupGiving.Core.Email
{
    public class SimpleSmtpEmailRelayService : IEmailRelayService
    {
        public void SendEmail(IEmailMessage emailMessage)
        {
            SmtpClient smtp = new SmtpClient();

            try
            {
                smtp.Send(emailMessage.ToMailMessage());
            } catch (Exception ex)
            {
                // TODO log error
            } finally
            {
                
            }
        }
    }
}