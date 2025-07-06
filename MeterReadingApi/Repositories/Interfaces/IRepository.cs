namespace MeterReadingApi.Repositories.Interfaces
{
    public interface IRepository<T> where T : IEntity
    {
        Task<T> Get(int globalId);
        Task<IEnumerable<T>> GetAll();
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int globalId);

    }
}
