using System.Runtime.Serialization;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Api.Controllers;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Areas.Api.Models
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