using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace GroupGiving.PayPal.Model
{
    [DataContract(Name="verifyPaypalAccountRequest", Namespace=GroupGiving.Core.Api.Namespace)]
    public class VerifyPaypalAccountRequest
    {
        [DataMember(Name = "firstName", EmitDefaultValue = false)]
        [Required]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName", EmitDefaultValue = false)]
        [Required]
        public string LastName { get; set; }

        [DataMember(Name = "email", EmitDefaultValue = false)]
        [Required]
        public string Email { get; set; }
    }
}