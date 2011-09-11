using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GroupGiving.Core.Services
{
    public class SetTicketDetailsRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage="Please select an event")]
        public int EventId { get; set; }

        [Required]
        [Range(1d, double.MaxValue, ErrorMessage="Ticket price must be greater than 0")]
        public decimal? TicketPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must provide a minimum number of participants")]
        public int? MinimumParticipants { get; set; }

        [Required]
        public int? MaximumParticipants { get; set; }

        [Required(ErrorMessage = "In order to receive money from ticket sales you must provide a PayPal account")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string PaypalAccountEmailAddress { get; set; }

        public DateTime SalesEndDateTime { get; set; }

        public string SalesEndDate { get; set; }

        public string SalesEndTime { get; set; }

        public string AdditionalBenefits { get; set; }

        public SelectList Times { get; set; }

        public SelectList SalesEndTimeOptions { get; set; }
    }
}