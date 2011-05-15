using System.Linq;
using GroupGiving.Core.Data.Azure;

namespace GroupGiving.Web.Models
{
    public class HomePageViewModel
    {
        public IQueryable<EventRow> Events { get; set; }
    }
}