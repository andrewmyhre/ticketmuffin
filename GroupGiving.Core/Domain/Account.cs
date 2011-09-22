using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupGiving.Core.Domain
{
    public class Account : IDomainObject
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string ResetPasswordToken { get; set; }
        public DateTime ResetPasswordTokenExpiry { get; set; }
        public AccountType AccountType { get; set; }
        public bool OptInForOffers { get; set; }

        public string PayPalEmail { get; set; }
        public string PayPalFirstName { get; set; }
        public string PayPalLastName { get; set; }
    }
}
