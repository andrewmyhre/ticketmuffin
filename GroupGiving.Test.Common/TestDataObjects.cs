using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Services;

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

    }
}
