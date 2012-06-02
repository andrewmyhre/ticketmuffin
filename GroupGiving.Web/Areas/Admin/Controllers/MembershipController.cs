using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class MembershipController : Controller
    {
        public MembershipController(IDocumentSession ravenSession)
        {
            ((RavenDBMembership.Provider.RavenDBMembershipProvider) Membership.Provider).DocumentStore =
                ravenSession.Advanced.DocumentStore;
        }

        public string Index()
        {
            StringBuilder output=new StringBuilder();
            int totalRecords;
            var users = Membership.Provider.GetAllUsers(0, 100, out totalRecords);
            output.AppendFormat("<p>{0} users</p>", users.Count);
            foreach(MembershipUser user in users)
            {
                output.AppendFormat("<p>{0}</p>", user.Email);
            }

            return output.ToString();
        }
    }
}