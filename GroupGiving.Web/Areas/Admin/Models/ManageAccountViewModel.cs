using System.Web.Security;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Areas.Admin.Models
{
    public class ManageAccountViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public AccountType AccountType { get; set; }
        public string PayPalEmail { get; set; }
        public string PayPalFirstName { get; set; }
        public string PayPalLastName { get; set; }

        public MembershipUser MembershipUser { get; set; }
    }
}