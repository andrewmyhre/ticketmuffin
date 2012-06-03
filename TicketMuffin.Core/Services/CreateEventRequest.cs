﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace GroupGiving.Core.Services
{
    public class CreateEventRequest
    {
        public HttpPostedFileBase ImageFile;

        [Required]
        [Display(Name="Title")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date, ErrorMessage="Please provide a valid date")]
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
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        [Required]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage="Please provide a url for your event")]
        [Display(Name = "http://www.ticketmuffin.com/")]
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

        public IEnumerable<SelectListItem> Countries { get; set; }
        [DataType(DataType.ImageUrl)]
        public string ImageFilename { get; set; }
        public string OrganiserAccountId { get; set; }

        public string OrganiserName { get; set; }

        public string ImageUrl { get; set; }
        public bool ForCharity { get; set; }

        public int? CharityId { get; set; }

        public string CharityLogoUrl { get; set; }

        public string CharityName { get; set; }

        public string CharityRegistrationNumber { get; set; }

        public string CharityDonationPageUrl { get; set; }

        public string CharityDonationGatewayName { get; set; }

        public string CharityDescription { get; set; }
    }
}