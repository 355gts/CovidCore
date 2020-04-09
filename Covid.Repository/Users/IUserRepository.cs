using Covid.Common.Model;
using Covid.Repository.Model.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid.Repository.Users
{
    public interface IUserRepository
    {
        Task<ResultStatus<long>> CreateUserAsync(User User);

        Task<bool> UpdateUserAsync(long UserId, User User);

        Task<bool> DeleteUserAsync(long UserId);

        Task<User> GetUserByIdAsync(long UserId);

        Task<IEnumerable<User>> GetUsersAsync();
    }
}
