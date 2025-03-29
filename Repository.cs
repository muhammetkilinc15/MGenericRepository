using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository
{
    public class Repository<TEntity, TContext>: IRepository<TEntity> where TEntity : class where TContext : DbContext
    {
        protected readonly TContext _Context;
        private readonly DbSet<TEntity> _Entity;
        public Repository(TContext context)
        {
            _Context = context;
            _Entity = _Context.Set<TEntity>();
        }

        public void Add(TEntity entity)
        {
            _Entity.Add(entity);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _Entity.AddAsync(entity, cancellationToken);
        }

        public void AddRange(ICollection<TEntity> entities)
        {
            _Entity.AddRangeAsync(entities);
        }

        public async Task AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _Entity.AddRangeAsync(entities, cancellationToken);
        }

        public bool Any(Expression<Func<TEntity, bool>> expression)
        {
            return _Entity.Any(expression);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.AnyAsync(expression, cancellationToken);
        }

        public IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.CountBy(expression);
        }

        public void Delete(TEntity entity)
        {
            _Entity.Remove(entity);
        }

        public async Task DeleteByExpressionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            TEntity entity = await _Entity.Where(expression).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            _Entity.Remove(entity);
        }

        public async Task DeleteByIdAsync(string id)
        {
            TEntity entity = await _Entity.FindAsync(id);
            _Entity.Remove(entity);
        }

        public void DeleteRange(ICollection<TEntity> entities)
        {
            _Entity.RemoveRange(entities);
        }

        public TEntity First(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return _Entity.First(expression);
            }

            return _Entity.AsNoTracking().First(expression);
        }

        public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return await _Entity.FirstAsync(expression, cancellationToken);
            }

            return await _Entity.AsNoTracking().FirstAsync(expression, cancellationToken);
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return _Entity.FirstOrDefault(expression);
            }

            return _Entity.AsNoTracking().FirstOrDefault(expression);
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return _Entity.FirstOrDefaultAsync(expression, cancellationToken);
            }
            return _Entity.AsNoTracking().FirstOrDefaultAsync(expression, cancellationToken);
        }

        public IQueryable<TEntity> GetAll(bool isTrackingActive = true, Expression<Func<TEntity, bool>> expression = null)
        {
            if (isTrackingActive)
            {
                return _Entity.Where(expression);
            }
            return _Entity.AsNoTracking().Where(expression);
        }

        public TEntity GetByExpression(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return _Entity.Where(expression).FirstOrDefault();
            }
            return _Entity.AsNoTracking().Where(expression).FirstOrDefault();
        }

        public async Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true, CancellationToken cancellationToken = default)
        {
            if (isTrackingActive)
            {
                return await _Entity.Where(expression).FirstOrDefaultAsync(cancellationToken);
            }
            return await _Entity.AsNoTracking().Where(expression).FirstOrDefaultAsync(cancellationToken);
        }

        public TEntity GetFirst()
        {
            return _Entity.First();
        }

        public async Task<TEntity> GetFirstAsync(CancellationToken cancellationToken = default)
        {
            return await _Entity.FirstAsync(cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _Entity.Update(entity);
        }

        public async Task UpdateByExpressionAsync(Expression<Func<TEntity, bool>> filterExpression,
                    Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression,
                         CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filterExpression);
            ArgumentNullException.ThrowIfNull(updateExpression);
            await _Entity.Where(filterExpression).ExecuteUpdateAsync(updateExpression, cancellationToken);
        }


        public void UpdateRange(ICollection<TEntity> entities)
        {
            _Entity.UpdateRange(entities);
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (isTrackingActive)
            {
                return _Entity.Where(expression);
            }
            return _Entity.AsNoTracking().Where(expression);
        }
    }
}
