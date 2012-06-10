using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public interface ITicketGenerator
    {
        void CreatePdfFile(GroupGivingEvent @event, out string outputPath);
    }
}
