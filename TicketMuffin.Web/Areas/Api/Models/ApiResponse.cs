using System.Runtime.Serialization;
using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Web.Areas.Api.Models
{
    [DataContract(Name = "response", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class ApiResponse<T>
    {
        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public ErrorResponse Errors { get; set; }

        [DataMember(Name="link", EmitDefaultValue = false)]
        public ResourceLink<T> Link { get; set; }
    }
}