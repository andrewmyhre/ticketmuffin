using System.Runtime.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name="paypalAccountVerification", Namespace=GroupGiving.Core.Api.Namespace)]
    public class VerifyPaypalAccountResponse
    {
        [DataMember(Name="success", EmitDefaultValue = true)]
        public bool Success { get; set; }

        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public GroupGiving.Core.Dto.ErrorResponse Errors { get; set; }

        [DataMember(Name="status")]
        public string AccountStatus { get; set; }
    }
}