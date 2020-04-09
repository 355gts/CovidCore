using Microsoft.EntityFrameworkCore;
using System;

namespace Covid.Repository.Factories
{
    public class CovidDbContextFactory : ICovidDbContextFactory
    {
        private readonly DbContextOptions<CovidDbContext> _options;

        public CovidDbContextFactory(DbContextOptions<CovidDbContext> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ICovidDbContext GetContext()
        {
            // todo pass username through in request
            return new CovidDbContext(_options);
        }
    }
}
