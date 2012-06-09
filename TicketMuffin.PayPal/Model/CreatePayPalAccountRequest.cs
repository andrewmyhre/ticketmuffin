using System;

namespace TicketMuffin.PayPal.Model
{
    public class CreatePayPalAccountRequest
    {
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string ReturnUrl { get; set; }
        public string ContactEmailAddress { get; set; }
        public string MerchantWebsiteAddress { get; set; }
        public DateTime OrganisationDateOfEstablisment { get; set; }
    }
}