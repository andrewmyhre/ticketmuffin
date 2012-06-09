using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using TicketMuffin.Web.Code;

namespace TicketMuffin.Web.Areas.Api.Models
{
    [DataContract(Name="shareViaEmail", Namespace=PayPal.Api.Namespace)]
    public class ShareViaEmailRequest
    {
        [CommaSeparatedEmailListValidator(ErrorMessage = "One or more of the email addresses you provided weren't valid")]
        [Required(ErrorMessage = "You need to provide at least one email address to send the email to")]
        [DataMember(Name="recipients")]
        public string Recipients { get; set; }

        [Required(ErrorMessage = "It is important that you provide a subject for the email")]
        [DataMember(Name="subject")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "It is important that you provide some content for the email")]
        [DataMember(Name = "body")]
        public string Body { get; set; }
    }
}