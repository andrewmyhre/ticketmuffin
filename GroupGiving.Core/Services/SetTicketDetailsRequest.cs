using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GroupGiving.Core.Services
{
    public class SetTicketDetailsRequest
    {
        [Required]
        public string ShortUrl { get; set; }

        [Required]
        [DisplayName("Ticket Price")]
        [Range(1d, double.MaxValue, ErrorMessage="Ticket price must be greater than 0")]
        public decimal? TicketPrice { get; set; }

        [Required]
        [DisplayName("Minimum Participants")]
        [Range(1, int.MaxValue, ErrorMessage = "You must provide a minimum number of participants")]
        public int? MinimumParticipants { get; set; }

        [Required]
        [DisplayName("Maximum Participants")]
        public int? MaximumParticipants { get; set; }

        
        public DateTime SalesEndDateTime { get; set; }

        [DisplayName("Sales End")]
        public string SalesEndDate { get; set; }

        public string SalesEndTime { get; set; }

        [DisplayName("Additional Benefits")]
        public string AdditionalBenefits { get; set; }

        public SelectList Times { get; set; }

        public SelectList SalesEndTimeOptions { get; set; }

        [Required(ErrorMessage = "In order to receive money from ticket sales you must provide a PayPal account")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "PayPal Account Email Address")]
        public string PayPalEmail { get; set; }

        public string PayPalFirstName { get; set; }

        public string PayPalLastName { get; set; }
    }
}