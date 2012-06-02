using System;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Admin.Models;
using Raven.Client;
using RavenDBMembership.Provider;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IDocumentSession _ravenSession;

        public UserManagementController(IDocumentSession ravenSession)
        {
            _ravenSession = ravenSession;
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
            userListViewModel.Users =
                users.Select(u => AutoMapper.Mapper.Map<Account, ManageAccountViewModel>(u))
                .ToList();

            foreach(var account in userListViewModel.Users)
            {
                account.MembershipUser = Membership.Provider.GetUser(account.Email, false);
            }

            return View(userListViewModel);
        }

        [ActionName("account")]
        public ActionResult ManageAccount(int id)
        {
            var manageAccountViewModel = new ManageAccountViewModel();

            AutoMapper.Mapper.CreateMap<Account, ManageAccountViewModel>();


            var account = _ravenSession.Load<Account>("accounts/"+id);
            manageAccountViewModel = AutoMapper.Mapper.Map<ManageAccountViewModel>(account);

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
                transactionScope.Complete();
            }

            return RedirectToAction("account", new {id});
        }
    }
}
