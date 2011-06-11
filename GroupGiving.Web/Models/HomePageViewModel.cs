using System.Collections.Generic;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Models
{
    public class HomePageViewModel
    {
        public IEnumerable<GroupGivingEvent> Events { get; set; }
    }
}