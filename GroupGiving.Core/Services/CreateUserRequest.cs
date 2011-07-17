using System;

namespace GroupGiving.Core.Services
{
    public class CreateUserRequest
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AddressLine1 { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }

        public string AccountPageUrl { get; set; }
    }
}