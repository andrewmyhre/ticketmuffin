using System;
using System.Linq;
using System.Transactions;
using System.Web.Security;

namespace TicketMuffin.Web.Models
{
    [Obsolete("Don't use this", true)]
    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;
        private readonly RoleProvider _roleProvider;

        public AccountMembershipService() : this(null, null)
        {
        }

        public AccountMembershipService(MembershipProvider provider, RoleProvider roleProvider)
        {
            _provider = provider ?? Membership.Provider;
            _roleProvider = roleProvider ?? Roles.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public MembershipUserCollection GetAllUsers()
        {
            int totalRecords;
            return _provider.GetAllUsers(0, 1000, out totalRecords);
        }

        public MembershipUser GetUser(string username)
        {
            return _provider.GetUser(username, false);
        }

        public string[] GetAllRoles()
        {
            return _roleProvider.GetAllRoles();
        }

        public string[] GetRolesForUser(string username)
        {
            return _roleProvider.GetRolesForUser(username);
        }

        public void AddRole(string roleName)
        {
            _roleProvider.CreateRole(roleName);
        }

        public void UpdateUser(MembershipUser user, string[] roles)
        {
            using (var ts = new TransactionScope())
            {
                _provider.UpdateUser(user);
                var existingRoles = _roleProvider.GetRolesForUser(user.UserName);
                if (roles != null && roles.Length > 0)
                {
                    var rolesToBeAdded = roles.Except(existingRoles).ToArray();
                    _roleProvider.AddUsersToRoles(new[] { user.UserName }, rolesToBeAdded);
                }
                if (existingRoles.Length > 0)
                {
                    var rolesToBeDeleted = (roles != null ? existingRoles.Except(roles) : existingRoles).ToArray();
                    _roleProvider.RemoveUsersFromRoles(new[] { user.UserName }, rolesToBeDeleted);
                }

                ts.Complete();
            }
        }

        public void DeleteRole(string roleName)
        {
            using (var ts = new TransactionScope())
            {
                // Delete role
                _roleProvider.DeleteRole(roleName, false);

                ts.Complete();
            }
        }
    }
}