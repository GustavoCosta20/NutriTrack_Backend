using Microsoft.EntityFrameworkCore;
using NutriTrack_Domains.Interfaces.Repository;
using System.Linq.Expressions;

namespace NutriTrack_Connection.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly Context _context;
        private readonly DbSet<TEntity> _entities;

        public Repository(Context context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _entities.FindAsync(id);
        }

        // --- CREATE ---
        public async Task AddAsync(TEntity entity)
        {
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void AddSync(TEntity entity)
        {
            _entities.Add(entity);
            _context.SaveChanges();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        // --- UPDATE ---
        public void Update(TEntity entity)
        {
            _entities.Attach(entity);
            // Muda estado entidade para Modificado
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _entities.Attach(entity);
            // Muda estado entidade para Modificado
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // --- DELETE ---
        public void Delete(TEntity entity)
        {
            _entities.Remove(entity);
            _context.SaveChanges();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _entities.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<TEntity> GetAll()
        {
            return _entities.AsQueryable();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.FirstOrDefaultAsync(predicate);
        }

        public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.FirstOrDefault(predicate);
        }

        public async Task<TEntity?> FirstOrDefaultAsync()
        {
            return await _entities.FirstOrDefaultAsync();
        }

        public TEntity? FirstOrDefault()
        {
            return _entities.FirstOrDefault();
        }
    }
}
