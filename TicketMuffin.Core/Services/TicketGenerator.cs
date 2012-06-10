using System;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class TicketGenerator : ITicketGenerator
    {
        public void CreatePdfFile(GroupGivingEvent @event, out string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}