using MeterReadingApi;
using MeterReadingApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeterReadingApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        protected StorageDbContext Context { get; set; }
        protected DbSet<T> DbSet { get; set; }

        public Repository(StorageDbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public async Task<T> Get(int accountId)
        {
            return await DbSet.Where(entity => entity.AccountId == accountId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await DbSet.ToListAsync();
        }

        public async Task Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entityToUpdate = await Get(entity.AccountId);
            if (entityToUpdate == null) 
            {
                throw new ArgumentException($"Unable to delete entity with accountId {entity.AccountId}. Entity doesn't exist");
            }

            DbSet.Update(entity);
            await Context.SaveChangesAsync();
        }

        public async Task Add(T entity)
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task Delete(int accountId)
        {
            var entityToRemove = await Get(accountId);

            if (entityToRemove == null)
            {
                throw new ArgumentException($"Unable to delete entity with accountId {accountId}. Entity doesn't exist");
            }

            DbSet.Remove(entityToRemove);
            await Context.SaveChangesAsync();
        }
    }
}
