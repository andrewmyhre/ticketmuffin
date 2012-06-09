using System.ComponentModel.DataAnnotations;
using TicketMuffin.Web.Code;

namespace TicketMuffin.Web.Models
{
    public class ShareViaEmailViewModel
    {
        public bool Success { get; set; }
        [CommaSeparatedEmailListValidator(ErrorMessage="One or more of the email addresses you provided weren't valid")]
        [Required(ErrorMessage="You need to provide at least one email address to send the email to")]
        public string Recipients { get; set; }

        [Required(ErrorMessage="It is important that you provide a subject for the email")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "It is important that you provide some content for the email")]
        public string Body { get; set; }

        public string ShortUrl { get; set; }

        public string ShareUrl { get; set; }
    }
}