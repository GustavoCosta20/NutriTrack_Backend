using System.Linq.Expressions;

namespace NutriTrack_Domains.Interfaces.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id);
        Task AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        void Update(TEntity entity);
        Task DeleteAsync(TEntity entity);
        void Delete(TEntity entity);
        IQueryable<TEntity> GetAll();
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FirstOrDefaultAsync();
        TEntity? FirstOrDefault();
        Task UpdateAsync(TEntity entity);
    }
}
