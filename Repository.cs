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
    public class Repository<TEntity, TContext> : IRepository<TEntity> where TEntity : class where TContext : DbContext
    {
        protected readonly TContext _Context;
        private readonly DbSet<TEntity> _Entity;
        public Repository(TContext context)
        {
            _Context = context;
            _Entity = _Context.Set<TEntity>();
        }

        #region Add
        public virtual void Add(TEntity entity)
        {
            _Entity.Add(entity);
        }
        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _Entity.AddAsync(entity, cancellationToken);
        }
        public virtual void AddRange(ICollection<TEntity> entities)
        {
            _Entity.AddRange(entities);
        }
        public virtual async Task AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _Entity.AddRangeAsync(entities, cancellationToken);
        }
        #endregion Add

        #region Delete
        public virtual void Delete(TEntity entity)
        {
            _Entity.Remove(entity);
        }

        public virtual async Task DeleteByExpressionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            TEntity entity = await _Entity.Where(expression).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            _Entity.Remove(entity);
        }

        public virtual async Task DeleteByIdAsync(string id)
        {
            TEntity entity = await _Entity.FindAsync(id);
            _Entity.Remove(entity);
        }
        public virtual void DeleteRange(ICollection<TEntity> entities)
        {
            _Entity.RemoveRange(entities);
        }
        #endregion Delete

        #region Update
        public virtual void Update(TEntity entity)
        {
            _Entity.Update(entity);
        }

        public virtual async Task UpdateByExpressionAsync(Expression<Func<TEntity, bool>> filterExpression,
                    Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression,
                         CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filterExpression);
            ArgumentNullException.ThrowIfNull(updateExpression);
            await _Entity.Where(filterExpression).ExecuteUpdateAsync(updateExpression, cancellationToken);
        }


        public virtual void UpdateRange(ICollection<TEntity> entities)
        {
            _Entity.UpdateRange(entities);
        }
        #endregion Update


        #region Query
        public IQueryable<TEntity> AsQueryable(bool isTrackingActive = false)
        {
            if (!isTrackingActive)
                return _Entity.AsNoTracking().AsQueryable();
            return _Entity.AsQueryable();
        }

        public virtual IQueryable<TEntity> GetAll(bool isTrackingActive = false, Expression<Func<TEntity, bool>> expression = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _Entity.AsQueryable();
            if (!isTrackingActive)
            {
                query = query.AsNoTracking();
            }
            if (expression != null)
            {
                query = query.Where(expression);
            }
            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }

        #endregion Query


        #region Control
        public virtual bool Any(Expression<Func<TEntity, bool>> expression)
        {
            return _Entity.Any(expression);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.AnyAsync(expression, cancellationToken);
        }

        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (!isTrackingActive)
            {
                return _Entity.AsNoTracking().Where(expression);
            }
            return _Entity.Where(expression);
        }
        #endregion Control

        #region Count

        public virtual IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.CountBy(expression);
        }
        #endregion Count

        #region Get
        public virtual TEntity First(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (!isTrackingActive)
            {
                return _Entity.First(expression);
            }

            return _Entity.AsNoTracking().First(expression);
        }

        public virtual async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true)
        {
            if (!isTrackingActive)
            {
                return await _Entity.FirstAsync(expression, cancellationToken);
            }

            return await _Entity.AsNoTracking().FirstAsync(expression, cancellationToken);
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true)
        {
            if (!isTrackingActive)
            {
                return _Entity.FirstOrDefault(expression);
            }

            return _Entity.AsNoTracking().FirstOrDefault(expression);
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true)
        {
            if (!isTrackingActive)
            {
                return await _Entity.AsNoTracking().FirstOrDefaultAsync(expression, cancellationToken);
            }
            return await _Entity.FirstOrDefaultAsync(expression, cancellationToken);
        }

        public virtual TEntity GetByExpression(
                      Expression<Func<TEntity, bool>> expression,
                      bool isTrackingActive = true,
                      params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _Entity;

            if (!isTrackingActive)
            {
                query = query.AsNoTracking();
            }


            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query.FirstOrDefault(expression);
        }


        public virtual async Task<TEntity> GetByExpressionAsync(
                 Expression<Func<TEntity, bool>> expression,
                 bool isTrackingActive = true,
                 CancellationToken cancellationToken = default,
                 params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _Entity;

            if (includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTrackingActive)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Where(expression)
                .FirstOrDefaultAsync(cancellationToken);
        }


        public virtual TEntity GetFirst()
        {
            return _Entity.First();
        }

        public virtual async Task<TEntity> GetFirstAsync(CancellationToken cancellationToken = default)
        {
            return await _Entity.FirstAsync(cancellationToken);
        }
        #endregion

    }
}
