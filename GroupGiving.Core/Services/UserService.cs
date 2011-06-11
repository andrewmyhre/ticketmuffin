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
                Email=request.Email
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
    }
}
