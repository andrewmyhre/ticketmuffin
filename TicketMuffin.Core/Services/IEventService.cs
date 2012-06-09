using System.Collections.Generic;
using EmailProcessing;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface IEventService
    {
        IEnumerable<GroupGivingEvent> List(int pageSize = 20, int pageIndex = 0);
        CreateEventResult CreateEvent(CreateEventRequest request);
        void SetTicketDetails(SetTicketDetailsRequest setTicketDetailsRequest);
        bool ShortUrlAvailable(string shortUrl);
        GroupGivingEvent Retrieve(int eventId);
        GroupGivingEvent Retrieve(string shortUrl);
        void SendEventInvitationEmails(IEmailPackageRelayer emailPackageRelayer, string recipients, string body, string subject);
    }
}