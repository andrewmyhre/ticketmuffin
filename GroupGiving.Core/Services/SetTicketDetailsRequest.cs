using System.ComponentModel.DataAnnotations;

namespace GroupGiving.Core.Services
{
    public class SetTicketDetailsRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage="Please select an event")]
        public int EventId { get; set; }
    }
}