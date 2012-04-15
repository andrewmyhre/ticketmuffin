using System.Runtime.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name = "error", Namespace = Api.Namespace)]
    public class Error
    {
        [DataMember(Name="field")]
        public string Field { get; set; }
        [DataMember(Name="message")]
        public string ErrorMessage { get; set; }
    }
}