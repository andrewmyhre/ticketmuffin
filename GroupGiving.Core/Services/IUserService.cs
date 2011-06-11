using System;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public interface IUserService
    {
        UserAccount CreateUser(CreateUserRequest request);
        SendPasswordResetResult SendPasswordResetEmail(string email, IEmailService emailService);
        SendThanksForRegisteringEmailResult SendThanksForRegisteringEmail(string emailAddress, string firstName, IEmailService emailService);
        UserAccount RetrieveAccountByPasswordResetToken(string resetPasswordToken);
        ResetPasswordResult ResetPassword(string resetPasswordToken, string newPassword);
        UserAccount RetrieveByEmailAddress(string email);
        void UpdateAccount(UserAccount account);
    }
}