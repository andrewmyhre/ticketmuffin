using System;
using GroupGiving.Core.Services;

namespace GroupGiving.Test.Integration
{
    public static class Helpers
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
