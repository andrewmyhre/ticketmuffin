using System.Collections.Generic;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class HomePageViewModel
    {
        public IEnumerable<GroupGivingEvent> Events { get; set; }
    }
}