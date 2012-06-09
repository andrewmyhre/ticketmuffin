using System;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Email;

namespace TicketMuffin.Core.Services
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
