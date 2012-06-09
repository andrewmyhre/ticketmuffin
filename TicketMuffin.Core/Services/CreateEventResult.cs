using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class CreateEventResult
    {
        public bool Success { get; set; }

        public GroupGivingEvent Event { get; set; }
    }
}