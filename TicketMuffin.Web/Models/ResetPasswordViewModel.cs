using System.ComponentModel.DataAnnotations;
using Microsoft.Web.Mvc;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Models
{
    public class ResetPasswordViewModel
    {
        public bool TokenNotValid { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}