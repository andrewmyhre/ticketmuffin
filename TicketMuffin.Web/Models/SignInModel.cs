using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Microsoft.Web.Mvc;

namespace TicketMuffin.Web.Models
{
    public class SignInModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public SelectList Countries { get; set; }

        public SelectList AccountTypes { get; set; }
    }
}