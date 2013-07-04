namespace TicketMuffin.Core.Services
{
    public interface IAuthenticationService
    {
        bool CreateCredentials(string emailAddress, string password, string confirmPassword);
        bool CredentialsValid(string emailAddress, string passwordAttempt);
    }
}