using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Microsoft.Web.Mvc;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Models
{
    public class ContactDetailsViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

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

        public bool UpdatedSuccessfully { get; set; }

        public SelectList Countries { get; set; }

        public SelectList AccountTypes { get; set; }
    }
}