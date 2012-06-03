using System;
using System.Collections.Generic;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    [Obsolete("Use EmailProcessing", true)]
    public class EmailCreationService : IEmailCreationService
    {
        public IEmailTemplate PasswordResetInstructionsEmail(string emailAddress, string firstName, string resetPasswordToken)
        {
            return null;
            //return new PasswordResetInstructionsEmail(emailAddress, firstName, resetPasswordToken);
        }

        public IEmailTemplate ThankYouForRegisteringEmail(string emailAddress, string firstName)
        {
            return null;
            //return new ThanksForRegisteringEmail(emailAddress, firstName);
        }

        public IEmailTemplate GetYourAccountStartedEmail(string emailAddress, string resetPasswordToken)
        {
            return null;
            //return new GetYourAccountStarted(emailAddress, resetPasswordToken);
        }

        public IEmailTemplate MinimumNumberOfAttendeesReached(GroupGivingEvent @event, EventPledge pledge)
        {
            return null;
            //return new MinimumAttendeesReached(@event, pledge);
        }
    }
}