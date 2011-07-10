using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public class EmailCreationService : IEmailCreationService
    {
        public IEmailMessage PasswordResetInstructionsEmail(string emailAddress, string firstName, string resetPasswordToken)
        {
            return new PasswordResetInstructionsEmail(emailAddress, firstName, resetPasswordToken);
        }

        public IEmailMessage ThankYouForRegisteringEmail(string emailAddress, string firstName)
        {
            return new ThanksForRegisteringEmail(emailAddress, firstName);
        }

        public IEmailMessage GetYourAccountStartedEmail(string emailAddress, string resetPasswordToken)
        {
            return new GetYourAccountStarted(emailAddress, resetPasswordToken);
        }
    }
}