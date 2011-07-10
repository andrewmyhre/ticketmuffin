using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IEmailCreationService _emailCreationService;
        private readonly IEmailRelayService _emailRelayService;

        public AccountService(IRepository<Account> accountRepository, 
            IEmailCreationService emailCreationService,
            IEmailRelayService emailRelayService)
        {
            _accountRepository = accountRepository;
            _emailCreationService = emailCreationService;
            _emailRelayService = emailRelayService;
        }

        public Account CreateUser(CreateUserRequest request)
        {
            Account account = new Account()
            {
                Email=request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                AddressLine = request.AddressLine1,
                City = request.City,
                PostCode = request.PostCode,
                Country =  request.Country
            };

            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();

            var thanksForRegisteringEmail = _emailCreationService.ThankYouForRegisteringEmail(request.Email, request.FirstName);
            _emailRelayService.SendEmail(thanksForRegisteringEmail);

            return account;
        }

        public SendPasswordResetResult SendPasswordResetEmail(string emailAddress, IEmailRelayService emailRelayService)
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
            var email = _emailCreationService.PasswordResetInstructionsEmail(account.Email, account.FirstName, account.ResetPasswordToken);
            emailRelayService.SendEmail(email);

            return SendPasswordResetResult.SuccessResult;
        }

        public SendPasswordResetResult SendGetYourAccountStartedEmail(string emailAddress, IEmailRelayService emailRelayService)
        {
            var account = _accountRepository.Retrieve(a => a.Email == emailAddress);
            if (account == null)
                return SendPasswordResetResult.AccountNotFoundResult;

            // generate a unique token
            string token = Guid.NewGuid().ToString().Replace("{", "").Replace("-", "");
            var otherAccounts = _accountRepository.Query(a => a.ResetPasswordToken == token);
            while (otherAccounts.Count() > 0)
            {
                token = Guid.NewGuid().ToString().Replace("{", "").Replace("-", "");
                otherAccounts = _accountRepository.Query(a => a.ResetPasswordToken == token);
            }
            account.ResetPasswordToken = token;
            account.ResetPasswordTokenExpiry = DateTime.MaxValue;
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();

            // send email
            var email = _emailCreationService.GetYourAccountStartedEmail(account.Email, account.ResetPasswordToken);
            emailRelayService.SendEmail(email);

            return SendPasswordResetResult.SuccessResult;
        }

        public Account RetrieveAccountByPasswordResetToken(string resetPasswordToken)
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

        public Account RetrieveByEmailAddress(string email)
        {
            var account = _accountRepository.Retrieve(a => a.Email == email);
            return account;
        }

        public void UpdateAccount(Account account)
        {
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();
        }

        public void CreateIncompleteAccount(string emailAddress, IEmailRelayService emailRelayService)
        {
            var account = new Account()
                              {
                                  Email = emailAddress,
                                  ResetPasswordToken = Guid.NewGuid().ToString(),
                                  ResetPasswordTokenExpiry = DateTime.MaxValue
                              };
            _accountRepository.SaveOrUpdate(account);
            _accountRepository.CommitUpdates();

            SendGetYourAccountStartedEmail(emailAddress, emailRelayService);
        }
    }
}
