using System.Runtime.Serialization;

namespace GroupGiving.Core.Dto
{
    [DataContract(Name = "error", Namespace = Core.Api.Namespace)]
    public class Error
    {
        [DataMember(Name="field")]
        public string Field { get; set; }
        [DataMember(Name="message")]
        public string ErrorMessage { get; set; }
    }
}