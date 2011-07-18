using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GroupGiving.Core.Services
{
    public class CreateEventRequest
    {
        [Required]
        [Display(Name="Title")]
        public string Title { get; set; }

        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public string Venue { get; set; }

        [Required]
        [Display(Name = "Address line")]
        public string AddressLine { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Event Url")]
        public string ShortUrl { get; set; }

        [Required]
        [Display(Name = "private event (Url is not published)")]
        public bool IsPrivate { get; set; }

        [Required]
        [Display(Name = "I want this event to be featured")]
        public bool IsFeatured { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Contact phone number")]
        [RegularExpression(@"[\+\(\)0-9x -]*", ErrorMessage="Please provide a valid phone number (numbers, plus symbol, dashes, parentheses and 'x' for extensions are okay)")]
        public string PhoneNumber { get; set; }

        public string StartDate { get; set; }
        public string StartTime { get; set; }

        public SelectList StartTimes { get; set; }

        public string OrganiserName { get; set; }
    }
}