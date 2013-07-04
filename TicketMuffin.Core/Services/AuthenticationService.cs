using System;
using System.Linq;
using System.Text;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDocumentSession _session;

        public AuthenticationService(IDocumentSession session)
        {
            _session = session;
        }

        public bool CreateCredentials(string emailAddress, string password, string confirmPassword)
        {
            if (!string.Equals(password, confirmPassword, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The passwords do not match");
            }

            var saltedHashedPassword = Encoding.UTF8.GetBytes(BCrypt.HashPassword(password, BCrypt.GenerateSalt()));
            var credentials = new Credentials() {Username = emailAddress, SaltedHashedPassword = saltedHashedPassword};
            _session.Store(credentials);
            _session.SaveChanges();
            return true;
        }

        public bool CredentialsValid(string emailAddress, string passwordAttempt)
        {
            var credentials = _session.Query<Credentials>().SingleOrDefault(c=>c.Username==emailAddress);
            if (credentials != null)
                return BCrypt.CheckPassword(passwordAttempt, Encoding.UTF8.GetString(credentials.SaltedHashedPassword));
            return false;
        }
    }
}