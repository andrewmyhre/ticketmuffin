using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Services;
using GroupGiving.Web.Models;

namespace GroupGiving.Test.Common
{
    public static class TestDataObjects
    {
        public static CreateUserRequest CreateValidCreateUserRequest()
        {
            return new CreateUserRequest()
            {
                FirstName = "firstname",
                LastName = "lastname",
                AddressLine1 = "address line",
                City = "city",
                Country = "country",
                Email = "email@server.com",
                PostCode = "postcode"
            };
        }

        public static CreateEventRequest ValidCreateEventDetails()
        {
            return new CreateEventRequest()
            {
                Title = "event title",
                DateAndTime = DateTime.Now.AddDays(10),
                Venue = "my house",
                AddressLine = "55 albion drive",
                City = "london",
                PostCode = "e8 4lt",
                Description = "my test event",
                ShortUrl = "testevent",
                IsPrivate = false,
                IsFeatured = true,
                PhoneNumber = "0123456789"
            };
        }
    }
}
