using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class EventCultureResolver: IEventCultureResolver
    {
        private readonly IDocumentSession _ravenSession;

        public EventCultureResolver(IDocumentSession ravenSession)
        {
            _ravenSession = ravenSession;
        }

        public string ResolveCulture(GroupGivingEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.OrganiserId))
            {
                var organiserAccount = _ravenSession.Load<Account>(@event.OrganiserId);

                if (organiserAccount != null)
                {
                    return organiserAccount.Culture;
                }
            }

            return "en-GB";
        }
    }
}