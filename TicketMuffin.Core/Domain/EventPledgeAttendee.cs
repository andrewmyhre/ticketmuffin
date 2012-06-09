namespace TicketMuffin.Core.Domain
{
    public class EventPledgeAttendee
    {
        public EventPledgeAttendee(string fullName)
        {
            this.FullName = fullName;
        }

        public EventPledgeAttendee(){}

        public string FullName { get; set; }
    }
}