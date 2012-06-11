using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class ContactDetailsViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        public AccountType AccountType { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please provide your first name")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please provide your last name")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Address Line")]
        public string AddressLine { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Town or City")]
        public string Town { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Post Code")]
        public string PostCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        public bool UpdatedSuccessfully { get; set; }

        public SelectList Countries { get; set; }

        public SelectList AccountTypes { get; set; }
    }
}