using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public interface IUserService
    {
        UserAccount CreateUser(CreateUserRequest request);
    }
}