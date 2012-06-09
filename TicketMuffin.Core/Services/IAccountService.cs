using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;

namespace TicketMuffin.Core.Services
{
    public interface IAccountService
    {
        Account CreateUser(CreateUserRequest request);
        SendPasswordResetResult SendPasswordResetEmail(string email, IEmailRelayService emailRelayService);
        SendPasswordResetResult SendGetYourAccountStartedEmail(string emailAddress, IEmailRelayService emailRelayService);
        Account RetrieveAccountByPasswordResetToken(string resetPasswordToken);
        ResetPasswordResult ResetPassword(string resetPasswordToken, string newPassword);
        Account CreateIncompleteAccount(string emailAddress, IEmailRelayService emailRelayService);
        Account GetById(string accountId);
        Account RetrieveByEmailAddress(string emailAddress);
    }
}