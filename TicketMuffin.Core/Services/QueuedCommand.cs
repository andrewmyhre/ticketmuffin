namespace TicketMuffin.Core.Services
{
    public class QueuedCommand
    {
        public enum Actions
        {
            ActivateEvent
        }

        public Actions Action { get; set; }
        public string EventId { get; set; }
    }
}