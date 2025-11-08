using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Interfaces.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id);
        Task AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        IQueryable<TEntity> GetAll();
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FirstOrDefaultAsync();
        TEntity? FirstOrDefault();
        Task UpdateAsync(TEntity entity);
    }
}
