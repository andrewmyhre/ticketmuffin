namespace TicketMuffin.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool CreateCredentials(string emailAddress, string password, string confirmPassword)
        {
            throw new System.NotImplementedException();
        }

        public bool CredentialsValid(string emailAddress, string passwordAttempt)
        {
            throw new System.NotImplementedException();
        }
    }
}