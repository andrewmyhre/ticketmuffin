using System;
using System.Web.Security;

namespace TicketMuffin.Web.Models
{
    [Obsolete("Don't use this", true)]
    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);

        MembershipUserCollection GetAllUsers();

        MembershipUser GetUser(string username);

        string[] GetAllRoles();

        string[] GetRolesForUser(string username);

        void AddRole(string roleName);

        void UpdateUser(MembershipUser user, string[] roles);

        void DeleteRole(string roleName);
    }
}