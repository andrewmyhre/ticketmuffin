namespace GroupGiving.Core.Email
{
    public interface IEmailRelayService
    {
        void SendEmail(IEmailMessage emailMessage);
    }
}