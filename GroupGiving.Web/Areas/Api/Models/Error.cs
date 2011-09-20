using System.Runtime.Serialization;

namespace GroupGiving.Web.Areas.Api.Models
{
    [DataContract(Name = "error", Namespace = Areas.Api.Code.Api.Namespace)]
    public class Error
    {
        [DataMember(Name="field")]
        public string Field { get; set; }
        [DataMember(Name="message")]
        public string ErrorMessage { get; set; }
    }
}