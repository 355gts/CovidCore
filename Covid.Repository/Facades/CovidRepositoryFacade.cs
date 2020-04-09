using Covid.Repository.Factories;
using Covid.Repository.Users;
using System;

namespace Covid.Repository.Facades
{
    public class CovidRepositoryFacade : ICovidRepositoryFacade
    {
        private readonly ICovidDbContextFactory _covidDbContextFactory;

        public IUserRepository Users { get; }

        public CovidRepositoryFacade(ICovidDbContextFactory covidDbContextFactory)
        {
            _covidDbContextFactory = covidDbContextFactory ?? throw new ArgumentNullException(nameof(covidDbContextFactory));

            Users = new UserRepository(_covidDbContextFactory);
        }
    }
}
