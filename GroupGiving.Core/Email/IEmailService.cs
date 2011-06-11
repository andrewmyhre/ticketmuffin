namespace GroupGiving.Core.Email
{
    public interface IEmailService
    {
        void SendEmail(IEmailMessage emailMessage);
    }
}