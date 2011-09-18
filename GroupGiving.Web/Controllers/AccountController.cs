using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using log4net;
using Ninject;
using RavenDBMembership;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class AccountController : ContentTypeAwareController
    {
        private ILog _log = LogManager.GetLogger("AccountController");
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private ICountryService _countryService;

        public AccountController()
        {
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            _countryService = MvcApplication.Kernel.Get<ICountryService>();
            ((RavenDBMembershipProvider) Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("signup-or-signin")]
        public ActionResult SignUpOrSignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Request.QueryString["returnUrl"] != null)
                    Response.Redirect(Request.QueryString["returnUrl"]);
                else
                    Response.Redirect("/");
            }

            var model = new SignInModel();
            model.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name");
            model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SignIn(SignInModel request, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                var model = new SignInModel();
                model.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name");
                model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
                if (Request.AcceptTypes.Contains("application/json"))
                    return Json(new LogOnResultViewModel() { Success = false });
                return View("signup-or-signin", model);
            }

            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore = RavenDbDocumentStore.Instance;
            if (_membershipService.ValidateUser(request.EmailAddress, request.Password))
            {
                _formsService.SignIn(request.EmailAddress, request.RememberMe);
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    if (Request.AcceptTypes.Contains("application/json"))
                        return Json(new LogOnResultViewModel() { Success = true, RedirectUrl = returnUrl });
                    return Redirect(returnUrl);
                }
                else
                {
                    if (Request.AcceptTypes.Contains("application/json"))
                        return Json(new LogOnResultViewModel() { Success = true, RedirectUrl = Url.Action("Index", "Home") });
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("login", "The combination of email and password you entered did not match any user");
                var model = new SignInModel();
                model.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name");
                model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
                if (Request.AcceptTypes.Contains("application/json"))
                    return Json(new LogOnResultViewModel() { Success = false });
                return View("signup-or-signin", model);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("signup")]
        public ActionResult SignUp(SignUpModel request)
        {
            if (!ModelState.IsValid)
            {
                var model = new SignInModel();
                model.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name");
                model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
                return View("signup-or-signin", model);
            }

            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore = RavenDbDocumentStore.Instance;
            var createResult = _membershipService.CreateUser(request.Email, request.NewPassword, request.Email);

            if (createResult == MembershipCreateStatus.Success)
            {
                var createAccountRequest = new CreateUserRequest()
                {
                    FirstName = request.FirstName,
                    LastName=request.LastName,
                    AddressLine1 = request.AddressLine,
                    City=request.Town,
                    Country=request.Country,
                    Email=request.Email,
                    PostCode=request.PostCode
                };

                _accountService.CreateUser(createAccountRequest);

                _formsService.SignIn(request.Email, false);
                return RedirectToRoute("CreateEvent_EventDetails");
            }
            else
            {
                ModelState.AddModelError("signup", "There was a problem with the details you provided");
                var model = new SignInModel();
                model.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name");
                model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
                return View("signup-or-signin", model);
            }
        }

        //
        // GET: /Account/LogOff

        public ActionResult SignOut()
        {
            _formsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        public ActionResult Register(string redirectUrl)
        {
            var viewModel = new RegisterModel()
            {
                RedirectUrl = redirectUrl,
                Countries = new SelectList(_countryService.RetrieveAllCountries()),
                AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)))
            };

            return View(viewModel);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                using (var transaction = new TransactionScope())
                {
                    try
                    {
                        var createStatus = _membershipService.CreateUser(model.Email, model.Password, model.Email);

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            // create a full user account record
                            var createUserRequest = new CreateUserRequest();
                            createUserRequest.FirstName = model.FirstName;
                            createUserRequest.LastName = model.LastName;
                            createUserRequest.Email = model.Email;
                            createUserRequest.AddressLine1 = model.AddressLine;
                            createUserRequest.City = model.Town;
                            createUserRequest.PostCode = model.PostCode;
                            createUserRequest.Country = model.Country;
                            var userService = MvcApplication.Kernel.Get<IAccountService>();
                            userService.CreateUser(createUserRequest);
                            transaction.Complete();

                            _formsService.SignIn(model.Email, false /* createPersistentCookie */);
                            if (!string.IsNullOrWhiteSpace(model.RedirectUrl))
                                return Redirect(model.RedirectUrl);
                            else
                                return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                        }
                    } catch (Exception ex)
                    {
                        // there was a problem creating the user
                        _log.Fatal(ex);
                        ModelState.AddModelError("", "Could not create your account at this time");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = _membershipService.MinPasswordLength;

            model.Countries = new SelectList(new SelectList(_countryService.RetrieveAllCountries()));
            model.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
            if (string.IsNullOrWhiteSpace(model.ReturnView))
                return View(model);

            return View(model.ReturnView, model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ForgotPassword(string email)
        {
            var userService = MvcApplication.Kernel.Get<IAccountService>();
            var emailService = MvcApplication.Kernel.Get<IEmailRelayService>();
            var result = userService.SendPasswordResetEmail(email, emailService);

            if (result.AccountNotFound)
            {
                ModelState.AddModelError("email", "We don't have an account registered with that email address...");
                return View();
            }

            return RedirectToAction("ForgotPasswordEmailSent");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ForgotPasswordEmailSent()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ResetPassword(string token)
        {
            var resetPasswordViewModel = new ResetPasswordViewModel();
            var userService = MvcApplication.Kernel.Get<IAccountService>();
            var account = userService.RetrieveAccountByPasswordResetToken(token);

            if (account==null)
            {
                resetPasswordViewModel.TokenNotValid = true;
                ModelState.AddModelError("token", "That token is invalid or has expired.");
                return View(resetPasswordViewModel);
            }
            resetPasswordViewModel.EmailAddress = account.Email;
            return View(resetPasswordViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ResetPassword(string token, ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordViewModel);
            }
            var userService = MvcApplication.Kernel.Get<IAccountService>();

            var accountService = MvcApplication.Kernel.Get<IAccountService>();
            var account = accountService.RetrieveAccountByPasswordResetToken(token);
            if (account == null)
            {
                return RedirectToAction("ResetPassword", new {token = token});
            }

            using (var session = RavenDbDocumentStore.Instance.OpenSession())
            {
                var q = from u in session.Query<User>()
                        where u.Username == account.Email 
                        select u;
                var user = q.SingleOrDefault();

                user.PasswordSalt = PasswordUtil.CreateRandomSalt();
                user.PasswordHash = PasswordUtil.HashPassword(resetPasswordViewModel.NewPassword, user.PasswordSalt);

                session.SaveChanges();
            }
            
            userService.ResetPassword(token, resetPasswordViewModel.NewPassword);

            return RedirectToAction("PasswordHasBeenReset");
        }

        public ActionResult PasswordHasBeenReset()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ContactDetails()
        {
            var viewModel = new ContactDetailsViewModel();
            var accountService = MvcApplication.Kernel.Get<IAccountService>();
            var membershipUser = Membership.GetUser(true);
            var account = accountService.RetrieveByEmailAddress(membershipUser.Email);
            viewModel.AccountType = account.AccountType;
            viewModel.AddressLine = account.AddressLine;
            viewModel.Country = account.Country;
            viewModel.Town = account.City;
            viewModel.FirstName = account.FirstName;
            viewModel.LastName = account.LastName;
            viewModel.PostCode = account.PostCode;
            viewModel.Email = account.Email;

            viewModel.Countries = new SelectList(new SelectList(_countryService.RetrieveAllCountries()));

            viewModel.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));
            
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ContactDetails(ContactDetailsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // update
                var accountService = MvcApplication.Kernel.Get<IAccountService>();
                var membershipUser = Membership.GetUser(true);
                var account = accountService.RetrieveByEmailAddress(membershipUser.Email);
                account.FirstName = viewModel.FirstName;
                account.LastName = viewModel.LastName;
                account.AccountType = viewModel.AccountType;
                account.AddressLine = viewModel.AddressLine;
                account.City = viewModel.Town;
                account.PostCode = viewModel.PostCode;
                account.Country = viewModel.Country;
                accountService.UpdateAccount(account);
                viewModel.UpdatedSuccessfully = true;
                viewModel.Email = account.Email;
            }

            viewModel.Countries = new SelectList(new SelectList(_countryService.RetrieveAllCountries()));

                    viewModel.AccountTypes = new SelectList(Enum.GetNames(typeof(AccountType)));

            return View(viewModel);
        }

        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            if (returnUrl.Length >= 6)
            {
                returnUrl = returnUrl.Substring(6);
            }

            return Redirect("/" + lang + returnUrl);
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
