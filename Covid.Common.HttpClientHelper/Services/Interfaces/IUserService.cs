using Covid.Common.HttpClientHelper.Model;
using Covid.Web.Model.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid.Common.HttpClientHelper.Services.Interfaces
{
    public interface IUserService
    {
        Task<AsyncResult<User>> GetUserByIdAsync(int id);

        Task<AsyncResult<IEnumerable<User>>> GetUsersAsync();

        Task<AsyncResult<int>> CreateUserAsync(CreateUser user);

        Task<bool> UpdateUserAsync(int id, User user);

        Task<bool> DeleteUserAsync(int id);


    }
}
