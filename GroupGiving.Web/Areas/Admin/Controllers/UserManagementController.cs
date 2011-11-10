using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IDocumentStore _documentStore;

        public UserManagementController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        //
        // GET: /Admin/UserManagement/

        public ActionResult Index()
        {
            var userListViewModel = new UserListViewModel();

            using (var session = _documentStore.OpenSession())
            {
                userListViewModel.Users = session.Query<Account>().Take(1024).ToList();
            }

            return View(userListViewModel);
        }

        [ActionName("account")]
        public ActionResult ManageAccount(int id)
        {
            var manageAccountViewModel = new ManageAccountViewModel();

            AutoMapper.Mapper.CreateMap<Account, ManageAccountViewModel>();


            using (var session = _documentStore.OpenSession())
            {
                var account = session.Load<Account>("accounts/"+id);
                manageAccountViewModel = AutoMapper.Mapper.Map<ManageAccountViewModel>(account);
            }

            return View(manageAccountViewModel);
        }

        [ActionName("account")]
        [HttpPost]
        public ActionResult ManageAccount(int id, ManageAccountViewModel model)
        {
            using (var session = _documentStore.OpenSession())
            {
                var account = session.Load<Account>("accounts/" + id);

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
                session.SaveChanges();
            }

            return RedirectToAction("account", new {id});
        }
    }

    public class ManageAccountViewModel
    {
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
    }

    public class UserListViewModel
    {
        public List<Account> Users { get; set; }
    }
}
