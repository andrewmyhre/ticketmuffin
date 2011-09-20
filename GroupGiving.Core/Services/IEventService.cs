using System.Collections.Generic;
using EmailProcessing;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public interface IEventService
    {
        IEnumerable<GroupGivingEvent> RetrieveAllEvents();
        CreateEventResult CreateEvent(CreateEventRequest request);
        void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest);
        bool ShortUrlAvailable(string shortUrl);
        GroupGivingEvent Retrieve(int eventId);
        GroupGivingEvent Retrieve(string shortUrl);
        void SendEventInvitationEmails(IEmailPackageRelayer emailPackageRelayer, string recipients, string body, string subject);
    }
}