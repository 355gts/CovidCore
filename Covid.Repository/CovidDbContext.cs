using Covid.Repository.Model.Users;
using Microsoft.EntityFrameworkCore;

namespace Covid.Repository
{
    public class CovidDbContext : DbContext, ICovidDbContext
    {
        private readonly string _connectionString;
        public virtual DbSet<User> Users { get; set; }


        public CovidDbContext(DbContextOptions<CovidDbContext> options)
            : base(options)
        {
        }

        private CovidDbContext()
        {

        }

        // these settings add/override the ones provided in Startup.cs
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public void SetValues<TEntity>(TEntity existingEntity, TEntity updatedEntity) where TEntity : class
        {
            Entry(existingEntity).CurrentValues.SetValues(updatedEntity);
        }

        public void SetModified<TEntity>(TEntity existingEntity) where TEntity : class
        {
            Entry(existingEntity).State = EntityState.Modified;
        }
    }
}
