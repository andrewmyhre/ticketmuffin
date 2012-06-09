using System;
using System.Collections.Generic;

namespace TicketMuffin.Service
{
    public class DailyProcessedMessageCount
    {
        public DayOfWeek Day
        {
            get;
            set;
        }

        public HashSet<ProcessedMessageCount> ProcessedMessageCountList
        {
            get;
            set;
        }
    }
}