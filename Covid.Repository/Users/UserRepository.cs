using Covid.Common.Model;
using Covid.Repository.Factories;
using Covid.Repository.Model.Users;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid.Repository.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UserRepository));

        private readonly ICovidDbContextFactory _covidDbContextFactory;

        public UserRepository(ICovidDbContextFactory covidDbContextFactory)
        {
            _covidDbContextFactory = covidDbContextFactory ?? throw new ArgumentNullException(nameof(covidDbContextFactory));
        }

        public async Task<ResultStatus<long>> CreateUserAsync(User User)
        {
            using (var dbContext = _covidDbContextFactory.GetContext())
            {
                try
                {
                    dbContext.Users.Add(User);
                    await dbContext.SaveChangesAsync();
                    return new ResultStatus<long>(true, User.Id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.Error($"An unexpected exception occurred saving User {User.Firstname} {User.Surname}, error details - '{ex.Message}'.");
                    return new ResultStatus<long>(false) { ErrorMessage = ex.Message };
                }
            }
        }

        public async Task<bool> DeleteUserAsync(long UserId)
        {
            using (var dbContext = _covidDbContextFactory.GetContext())
            {
                try
                {
                    var existingUser = new User() { Id = UserId };

                    dbContext.Users.Attach(existingUser);
                    dbContext.Users.Remove(existingUser);

                    await dbContext.SaveChangesAsync();

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    _logger.Error($"An unexpected exception occurred deleting User with id '{UserId}', error details - '{ex.Message}'.");
                    return false;
                }

            }
        }

        public async Task<User> GetUserByIdAsync(long UserId)
        {
            try
            {
                using (var dbContext = _covidDbContextFactory.GetContext())
                {
                    return await dbContext.Users.FirstOrDefaultAsync(c => c.Id == UserId);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.Error($"An unexpected exception occurred updating User with id '{UserId}', error details - '{ex.Message}'.");
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            try
            {
                using (var dbContext = _covidDbContextFactory.GetContext())
                {
                    return await dbContext.Users.ToListAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.Error($"An unexpected exception occurred getting users, error details - '{ex.Message}'.");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(long UserId, User User)
        {
            using (var dbContext = _covidDbContextFactory.GetContext())
            {
                try
                {
                    var existingUser = await dbContext.Users.FirstOrDefaultAsync(c => c.Id == UserId);
                    if (existingUser == null)
                        return false;

                    dbContext.SetValues(existingUser, User);
                    dbContext.SetModified(existingUser);

                    await dbContext.SaveChangesAsync();

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    _logger.Error($"An unexpected exception occurred updateing User with id '{UserId}', error details - '{ex.Message}'.");
                    return false;
                }
            }
        }
    }
}
