using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserAccount> _userRepository;

        public UserService(IRepository<UserAccount> userRepository)
        {
            _userRepository = userRepository;
        }

        public UserAccount CreateUser(CreateUserRequest request)
        {
            UserAccount userAccount = new UserAccount()
            {
                Email=request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                PostCode = request.PostCode,
                Country =  request.Country
            };

            _userRepository.SaveOrUpdate(userAccount);

            return userAccount;
        }
    }

    public class UpdateUserRequest
    {
    }

    public class CreateUserRequest
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AddressLine1 { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }
    }
}
