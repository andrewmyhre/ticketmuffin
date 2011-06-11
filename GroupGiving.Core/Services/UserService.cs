﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserAccount> _accountRepository;

        public UserService(IRepository<UserAccount> accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public UserAccount CreateUser(CreateUserRequest request)
        {
            UserAccount userAccount = new UserAccount()
            {
                Email=request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                PostCode = request.PostCode,
                Country =  request.Country
            };

            _accountRepository.SaveOrUpdate(userAccount);
            _accountRepository.CommitUpdates();

            return userAccount;
        }

        public SendPasswordResetResult SendPasswordResetEmail(string emailAddress, IEmailService emailService)
        {
            var account = _accountRepository.Retrieve(a => a.Email == emailAddress);
            if (account == null)
                return SendPasswordResetResult.AccountNotFoundResult;

            // generate a unique token
            string token = Guid.NewGuid().ToString().Replace("{", "").Replace("-", "");
            var otherAccounts = _accountRepository.Query(a => a.ResetPasswordToken == token);
            while(otherAccounts.Count() > 0)
            {
                token = Guid.NewGuid().ToString().Replace("{", "").Replace("-", "");
                otherAccounts = _accountRepository.Query(a => a.ResetPasswordToken == token);
            }
            account.ResetPasswordToken = token;
            account.ResetPasswordTokenExpiry = DateTime.Now.AddDays(1);
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();

            // send email
            var email = new PasswordResetInstructionsEmail(account.Email, account.FirstName, account.ResetPasswordToken);
            emailService.SendEmail(email);

            return SendPasswordResetResult.SuccessResult;
        }

        public SendThanksForRegisteringEmailResult SendThanksForRegisteringEmail(string emailAddress, string firstName, IEmailService emailService)
        {
            var thanksForRegisteringEmail = new ThanksForRegisteringEmail(emailAddress, firstName);
            emailService.SendEmail(thanksForRegisteringEmail);

            return SendThanksForRegisteringEmailResult.SuccessResult;
        }

        public UserAccount RetrieveAccountByPasswordResetToken(string resetPasswordToken)
        {
            return _accountRepository
                .Retrieve(a => a.ResetPasswordToken == resetPasswordToken
                               && a.ResetPasswordTokenExpiry > DateTime.Now);
        }

        public ResetPasswordResult ResetPassword(string resetPasswordToken, string newPassword)
        {
            var account = RetrieveAccountByPasswordResetToken(resetPasswordToken);
            if (account == null) return ResetPasswordResult.InvalidTokenResult;

            account.ResetPasswordToken = null;
            account.ResetPasswordTokenExpiry = DateTime.MinValue;
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();

            return ResetPasswordResult.SuccessResult;
        }

        public UserAccount RetrieveByEmailAddress(string email)
        {
            var account = _accountRepository.Retrieve(a => a.Email == email);
            return account;
        }

        public void UpdateAccount(UserAccount account)
        {
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();
        }
    }

    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public bool InvalidToken { get; set; }

        public static ResetPasswordResult InvalidTokenResult
        {
            get
            {
                return new ResetPasswordResult() { InvalidToken = true };
            }
        }

        public static ResetPasswordResult SuccessResult
        {
            get { return new ResetPasswordResult(){Success=true};}
        }

    }
}
