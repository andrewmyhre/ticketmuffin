using System.Collections.Generic;
using System.Web.Security;

namespace TicketMuffin.Web.Areas.Admin.Models
{
    public class UserListViewModel
    {
        public List<ManageAccountViewModel> Users { get; set; }

        public List<ManageAccountViewModel> AccountOrphans { get; set; }

        public List<MembershipUser> MembershipOrphans { get; set; }
    }
}