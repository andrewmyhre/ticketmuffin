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
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class AccountController : Controller
    {
        private ILog _log = LogManager.GetLogger("AccountController");
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;

        public AccountController()
        {
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            ((RavenDBMembershipProvider) Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        //
        // GET: /Account/LogOn

        public ActionResult SignIn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult SignIn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    _formsService.SignIn(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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
                RedirectUrl = redirectUrl
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
                            var userAccountRepository = MvcApplication.Kernel.Get<IUserService>();
                            var userAccount = userAccountRepository.CreateUser(createUserRequest);

                            // send a registration email to the user
                            var thanksForRegisteringEmail = new ThanksForRegisteringEmail(model.Email, model.Email);
                            MvcApplication.Kernel.Get<IEmailService>().SendEmail(thanksForRegisteringEmail);

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
            return View(model);
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
