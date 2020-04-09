using Covid.Repository.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Covid.Repository
{
    public interface ICovidDbContext : IDisposable
    {
        DbSet<User> Users { get; set; }

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        void SetValues<TEntity>(TEntity existingEntity, TEntity updatedEntity) where TEntity : class;

        void SetModified<TEntity>(TEntity existingEntity) where TEntity : class;

        EntityEntry Entry(object entity);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
