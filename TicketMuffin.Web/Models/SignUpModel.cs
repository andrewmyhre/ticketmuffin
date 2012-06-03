using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Code;
using Microsoft.Web.Mvc;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Models
{
    public class SignUpModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public AccountType AccountType { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Address Line")]
        [Required]
        public string AddressLine { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Town or City")]
        [Required]
        public string Town { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Post Code")]
        [Required]
        public string PostCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; }

        [Required]
        [Display(Name = "I agree to the Terms and Conditions")]
        [MustBeTrue(ErrorMessage = "You must agree to the terms and conditions")]
        public bool AgreeToTermsAndConditions { get; set; }

        public string RedirectUrl { get; set; }

        public SelectList Countries { get; set; }

        public SelectList AccountTypes { get; set; }
    }
}