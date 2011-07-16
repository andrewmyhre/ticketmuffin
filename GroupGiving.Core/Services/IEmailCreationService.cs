using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;

namespace GroupGiving.Core.Services
{
    public interface IEmailCreationService
    {
        IEmailMessage PasswordResetInstructionsEmail(string emailAddress, string firstName, string resetPasswordToken);
        IEmailMessage ThankYouForRegisteringEmail(string emailAddress, string firstName);
        IEmailMessage GetYourAccountStartedEmail(string emailAddress, string resetPasswordToken);
        IEmailMessage MinimumNumberOfAttendeesReached(GroupGivingEvent @event, EventPledge pledge);
    }
}
