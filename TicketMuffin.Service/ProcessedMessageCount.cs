namespace TicketMuffin.Service
{
    public class ProcessedMessageCount
    {
        public int HourOfDay
        {
            get;
            set;
        }

        public string HourLabel
        {
            get;
            set;
        }

        public int MessageCount
        {
            get;
            set;
        }
    }
}