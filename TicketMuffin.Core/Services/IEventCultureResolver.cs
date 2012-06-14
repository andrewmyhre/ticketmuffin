using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface IEventCultureResolver
    {
        string ResolveCulture(GroupGivingEvent @event);
    }
}