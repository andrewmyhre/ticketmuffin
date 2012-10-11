using System;
using System.Linq;
using EmailProcessing;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;

namespace TicketMuffin.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IEmailFacade _emailFacade;
        private readonly IDocumentSession _documentSession;

        public AccountService(IEmailFacade emailFacade,
            IDocumentSession documentSession)
        {
            _emailFacade = emailFacade;
            _documentSession = documentSession;
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

            _documentSession.Store(account);
            _documentSession.SaveChanges();

            _emailFacade.Send(
                request.Email, 
                "AccountCreated", 
                new {
                    Account=account,
                    AccountPageUrl = request.AccountPageUrl
                },
                "pl");

            return account;
        }

        public SendPasswordResetResult SendPasswordResetEmail(string emailAddress, IEmailRelayService emailRelayService)
        {
            var account = RetrieveByEmailAddress(emailAddress);
            if (account == null)
                return SendPasswordResetResult.AccountNotFoundResult;

            var token = GenerateAUniquePasswordResetToken();

            account.ResetPasswordToken = token;
            account.ResetPasswordTokenExpiry = DateTime.Now.AddDays(1);
            _documentSession.SaveChanges();

            // send email
            _emailFacade.Send(account.Email, "ResetYourPassword", new { Account = account }, "pl");

            return SendPasswordResetResult.SuccessResult;
        }

        private string GenerateAUniquePasswordResetToken()
        {
            string token = "";
            do
            {
                token = Guid.NewGuid().ToString().Replace("{", "").Replace("-", "");
            } while (_documentSession.Query<Account>().Any(a => a.ResetPasswordToken == token));
            return token;
        }

        public Account RetrieveByEmailAddress(string emailAddress)
        {
            var account = _documentSession.Query<Account>().SingleOrDefault(a => a.Email == emailAddress);
            return account;
        }

        public SendPasswordResetResult SendGetYourAccountStartedEmail(string emailAddress, IEmailRelayService emailRelayService)
        {
            var account = RetrieveByEmailAddress(emailAddress);
            if (account == null)
                return SendPasswordResetResult.AccountNotFoundResult;

            var token = GenerateAUniquePasswordResetToken(); account.ResetPasswordToken = token;
            account.ResetPasswordTokenExpiry = DateTime.MaxValue;
            _documentSession.SaveChanges();

            // send email
            _emailFacade.Send(account.Email, "GetYourAccountStarted",
                new { Account = account }, "pl");

            return SendPasswordResetResult.SuccessResult;
        }

        public Account RetrieveAccountByPasswordResetToken(string resetPasswordToken)
        {
            return _documentSession.Query<Account>()
                .SingleOrDefault(a => a.ResetPasswordToken == resetPasswordToken
                               && a.ResetPasswordTokenExpiry > DateTime.Now);
        }

        public ResetPasswordResult ResetPassword(string resetPasswordToken, string newPassword)
        {
            var account = RetrieveAccountByPasswordResetToken(resetPasswordToken);
            if (account == null) return ResetPasswordResult.InvalidTokenResult;

            account.ResetPasswordToken = null;
            account.ResetPasswordTokenExpiry = DateTime.MinValue;
            _documentSession.SaveChanges();

            return ResetPasswordResult.SuccessResult;
        }

        public Account CreateAnonymousAccountPendingTransaction(string transactionId, bool optInForSpecialOffers)
        {
            var account = new Account()
                                {
                                    PendingTransactionId = transactionId,
                                    OptInForOffers = optInForSpecialOffers
                                };
            _documentSession.Store(account);

            return account;
        }

        public Account CreateIncompleteAccount(string emailAddress, IEmailRelayService emailRelayService)
        {
            var account = new Account()
            {
                Email = emailAddress,
                ResetPasswordToken = Guid.NewGuid().ToString(),
                ResetPasswordTokenExpiry = DateTime.MaxValue
            };
            _documentSession.Store(account);

            SendGetYourAccountStartedEmail(emailAddress, emailRelayService);

            return account;
        }

        public Account GetById(string accountId)
        {
            return _documentSession.Load<Account>(accountId);
        }
        public Account GetById(int accountId)
        {
            return _documentSession.Load<Account>(accountId);
        }
    }
}
