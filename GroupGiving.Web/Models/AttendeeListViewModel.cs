using System.Collections.Generic;

namespace GroupGiving.Web.Models
{
    public class AttendeeListViewModel
    {
        public IEnumerable<AttendeeViewModel> Attendees { get; set; }

        public string EventName { get; set; }

        public string ShortUrl { get; set; }
    }

    public class AttendeeViewModel
    {
        public string FullName { get; set; }
        public string OrderNumber { get; set; }
    }
}