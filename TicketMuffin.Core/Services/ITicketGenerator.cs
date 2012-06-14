using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface ITicketGenerator
    {
        void CreatePdfFile(GroupGivingEvent @event, out string outputPath);
        Stream CreatePdf(GroupGivingEvent @event, EventPledge pledge, EventPledgeAttendee attendee, string culture);
    }
}
