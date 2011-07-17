using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    [Obsolete("Use EmailProcessing", true)]
    public interface IEmailCreationService
    {
        IEmailTemplate PasswordResetInstructionsEmail(string emailAddress, string firstName, string resetPasswordToken);
        IEmailTemplate ThankYouForRegisteringEmail(string emailAddress, string firstName);
        IEmailTemplate GetYourAccountStartedEmail(string emailAddress, string resetPasswordToken);
        IEmailTemplate MinimumNumberOfAttendeesReached(GroupGivingEvent @event, EventPledge pledge);
    }
}
