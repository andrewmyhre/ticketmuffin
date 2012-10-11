using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using Raven.Client;
using RavenDBMembership.Provider;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.Areas.Admin.Models;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IDocumentSession _ravenSession;
        private readonly IAccountService _accountService;
        private readonly MembershipProvider _membershipProvider;
        private readonly RoleProvider _roleProvider;

        public UserManagementController(IDocumentSession ravenSession, IAccountService accountService, 
            MembershipProvider membershipProvider, RoleProvider roleProvider)
        {
            _ravenSession = ravenSession;
            _accountService = accountService;
            _membershipProvider = membershipProvider;
            _roleProvider = roleProvider;
            AutoMapper.Mapper.CreateMap<Account, ManageAccountViewModel>();
        }

        //
        // GET: /Admin/UserManagement/

        public ActionResult Index()
        {
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = _ravenSession.Advanced.DocumentStore;

            var userListViewModel = new UserListViewModel();

            var users = _ravenSession.Query<Account>().Take(1024).ToList();
            userListViewModel.Users = new List<ManageAccountViewModel>();

            int totalRecords;
            var membershipUsers = _membershipProvider.GetAllUsers(0, 100, out totalRecords);
            foreach(MembershipUser membership in membershipUsers)
            {
                var account = users.FirstOrDefault(a => a.Email == membership.UserName);
                if(account == null)
                {
                    account = new Account()
                        {
                            Email=membership.UserName
                        };
                    _ravenSession.Store(account);
                    _ravenSession.SaveChanges();
                }
                
                var accountViewModel = AutoMapper.Mapper.Map<Account, ManageAccountViewModel>(account);
                accountViewModel.Roles = string.Join(", ",_roleProvider.GetRolesForUser(membership.UserName));
                accountViewModel.MembershipUser = membership;
                userListViewModel.Users.Add(accountViewModel);
            }

            userListViewModel.AccountOrphans = new List<ManageAccountViewModel>();
            var membershipUserList = new List<MembershipUser>();
            foreach(MembershipUser membershipUser in membershipUsers)
                membershipUserList.Add(membershipUser);

            userListViewModel.AccountOrphans = users
                .Where(u => membershipUserList.All(mu => mu.UserName != u.Email))
                .Select(AutoMapper.Mapper.Map<Account, ManageAccountViewModel>)
                .ToList();

            userListViewModel.MembershipOrphans =
                membershipUserList.Where(mu => users.All(u => u.Email != mu.UserName))
                    .ToList();


            return View(userListViewModel);
        }

        [ActionName("account")]
        public ActionResult ManageAccount(int id)
        {
            var manageAccountViewModel = new ManageAccountViewModel();

            AutoMapper.Mapper.CreateMap<Account, ManageAccountViewModel>();


            var account = _ravenSession.Load<Account>(id);
            manageAccountViewModel = AutoMapper.Mapper.Map<ManageAccountViewModel>(account);
            manageAccountViewModel.MembershipUser = _membershipProvider.GetUser(account.Email, false);
            manageAccountViewModel.Roles = string.Join(", ", _roleProvider.GetRolesForUser(account.Email));

            return View(manageAccountViewModel);
        }

        [ActionName("account")]
        [HttpPost]
        public ActionResult ManageAccount(int id, ManageAccountViewModel model)
        {
            using (var transactionScope = new TransactionScope())
            {
                var account = _ravenSession.Load<Account>("accounts/" + id);
                string previousEmailAddress = account.Email;

                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.AddressLine = model.AddressLine;
                account.City = model.City;
                account.PostCode = model.PostCode;
                account.Country = model.Country;
                account.PayPalEmail = model.PayPalEmail;
                account.PayPalFirstName = model.PayPalFirstName;
                account.PayPalLastName = model.PayPalLastName;

                // update an events related to this user
                var events = _ravenSession.Query<GroupGivingEvent>()
                    .Where(e => e.OrganiserId == account.Id);
                foreach (var @event in events)
                    @event.OrganiserName = string.Format("{0} {1}", model.FirstName, model.LastName);

                // update pledges from this user
                var eventsWithPledgesByThisUser =
                    _ravenSession.Query<GroupGivingEvent>()
                        .Where(e => e.Pledges.Any(p => p.AccountEmailAddress == previousEmailAddress));

                foreach (var @event in eventsWithPledgesByThisUser)
                {
                    var pledges = @event.Pledges.Where(p => p.AccountEmailAddress == previousEmailAddress);
                    foreach(var pledge in pledges)
                    {
                        pledge.AccountEmailAddress = model.Email;
                        pledge.AccountName = string.Format("{0} {1}", model.FirstName, model.LastName);
                    }
                }

                _ravenSession.SaveChanges();

                if (!string.IsNullOrWhiteSpace(model.Roles))
                {
                    var membershipUser = _membershipProvider.GetUser(model.Email, false);


                    var roles = model.Roles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(r=>r.Trim());
                    foreach (var role in roles)
                    {
                        if (!_roleProvider.RoleExists(role))
                        {
                            _roleProvider.CreateRole(role);
                        }
                    }

                    var currentRoles = _roleProvider.GetRolesForUser(membershipUser.UserName);
                    var notInRoles = currentRoles.Where(cr => !roles.Contains(cr));
                    _roleProvider.RemoveUsersFromRoles(new[] { model.Email }, notInRoles.ToArray());

                    var newRoles = roles.Where(selectedRole => !currentRoles.Contains(selectedRole));
                    _roleProvider.AddUsersToRoles(new[] { model.Email }, newRoles.ToArray());
                }

                transactionScope.Complete();
            }

            return RedirectToAction("account", new {id});
        }

        public ActionResult FixNonMembership(int id)
        {
            var account = _accountService.GetById(id);
            if (account == null)
                return RedirectToAction("Index");

            dynamic viewModel = new ExpandoObject();
            viewModel.AccountId = id;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult FixNonMembership(int AccountId, string newPassword)
        {
            var account = _accountService.GetById(AccountId);
            if (account == null)
                return RedirectToAction("Index");

            MembershipCreateStatus status;
            _membershipProvider.CreateUser(account.Email, newPassword, account.Email, "", "", true, AccountId,
                                           out status);

            if (status == MembershipCreateStatus.Success)
            {
                return RedirectToAction("Index");
            }

            return new ContentResult(){Content = status.ToString(), ContentType = "text/xml"};
        }
    }
}
