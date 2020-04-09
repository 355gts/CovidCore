using Covid.Common.HttpClientHelper.Model;
using Covid.Common.HttpClientHelper.Services.Interfaces;
using Covid.Web.Model.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid.Common.HttpClientHelper.Services
{
    public class UserService : IUserService
    {
        private const string _apiVersion = "2.0";
        private readonly IHttpClientHelper _httpClientHelper;

        public UserService(IHttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper ?? throw new ArgumentNullException(nameof(httpClientHelper));
        }

        public async Task<AsyncResult<int>> CreateUserAsync(CreateUser user)
        {
            string resourceUri = $"api/users/create";

            return await _httpClientHelper.PostAsync(resourceUri, user, _apiVersion).ConfigureAwait(false);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            string resourceUri = $"api/users/{id}";

            return await _httpClientHelper.DeleteAsync(resourceUri, _apiVersion).ConfigureAwait(false);
        }

        public async Task<AsyncResult<User>> GetUserByIdAsync(int id)
        {
            string resourceUri = $"api/users/{id}";

            return await _httpClientHelper.GetAsync<User>(resourceUri, _apiVersion).ConfigureAwait(false);
        }

        public async Task<AsyncResult<IEnumerable<User>>> GetUsersAsync()
        {
            string resourceUri = $"api/users";

            return await _httpClientHelper.GetAsync<IEnumerable<User>>(resourceUri, _apiVersion).ConfigureAwait(false);
        }

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            string resourceUri = $"api/users/{id}";

            return await _httpClientHelper.PutAsync(resourceUri, user, _apiVersion).ConfigureAwait(false);
        }
    }
}
