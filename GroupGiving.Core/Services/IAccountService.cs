using System;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public interface IAccountService
    {
        Account CreateUser(CreateUserRequest request);
        SendPasswordResetResult SendPasswordResetEmail(string email, IEmailRelayService emailRelayService);
        SendPasswordResetResult SendGetYourAccountStartedEmail(string emailAddress, IEmailRelayService emailRelayService);
        Account RetrieveAccountByPasswordResetToken(string resetPasswordToken);
        ResetPasswordResult ResetPassword(string resetPasswordToken, string newPassword);
        Account RetrieveByEmailAddress(string email);
        void UpdateAccount(Account account);
        Account CreateIncompleteAccount(string emailAddress, IEmailRelayService emailRelayService);
    }
}