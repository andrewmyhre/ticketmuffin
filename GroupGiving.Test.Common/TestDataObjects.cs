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
                FirstName = "ui test user",
                LastName = "lastname",
                AddressLine1 = "address line",
                City = "city",
                Country = "country",
                Email = Guid.NewGuid().ToString()+"@server.com",
                PostCode = "postcode"
            };
        }

        public static CreateEventRequest ValidCreateEventDetails()
        {
            return new CreateEventRequest()
            {
                Title = "event title",
                StartDateTime = DateTime.Now.AddDays(10),
                Venue = "my house",
                AddressLine = "55 albion drive",
                City = "london",
                Description = "my test event",
                ShortUrl = "testevent",
                IsPrivate = false,
                IsFeatured = true,
                PhoneNumber = "0123456789"
            };
        }
    }
}
