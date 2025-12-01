using GenericRepository.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GenericRepository.Services
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
        // test

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
            if (entity is null)
                throw new RepositoryException("Entity cannot be null.");

            _Entity.Remove(entity);
        }

        public virtual async Task DeleteByExpressionAsync(
            Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default)
        {
            TEntity? entity = await _Entity
                .Where(expression)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (entity is null)
                throw new RepositoryException("Entity not found with the given expression.");

            _Entity.Remove(entity);
        }

        public virtual async Task DeleteByIdAsync(string id)
        {
            TEntity? entity = await _Entity.FindAsync(id);

            if (entity is null)
                throw new RepositoryException($"Entity with Id {id} not found.");

            _Entity.Remove(entity);
        }

        public virtual void DeleteRange(ICollection<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new RepositoryException("No entities provided to delete.");

            _Entity.RemoveRange(entities);
        }
        #endregion Delete
        #region Update
        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new RepositoryException("Entity cannot be null.");

            var entry = _Context.Entry(entity);
            if (entry.State == EntityState.Detached)
                throw new RepositoryException("Cannot update a detached entity.");

            _Entity.Update(entity);
        }


        public virtual async Task UpdateByExpressionAsync(
             Expression<Func<TEntity, bool>> filterExpression,
             Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression,
             CancellationToken cancellationToken = default)
                {
                    ArgumentNullException.ThrowIfNull(filterExpression);
                    ArgumentNullException.ThrowIfNull(updateExpression);

                    var affectedRows = await _Entity
                        .Where(filterExpression)
                        .ExecuteUpdateAsync(updateExpression, cancellationToken);

                    if (affectedRows == 0)
                        throw new RepositoryException("No entities matched the filter expression to update.");
        }


        public virtual void UpdateRange(ICollection<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new RepositoryException("No entities provided to update.");

            _Entity.UpdateRange(entities);
        }

        #endregion Update
        #region Query
        public IQueryable<TEntity> Query(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null,
            bool isTrackingActive = false)
        {
            IQueryable<TEntity> query = _Entity;

            if (!isTrackingActive)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includeFunc != null)
                query = includeFunc(query);

            return query;
        }

        public async Task<List<TDto>> GetListAsync<TDto>(
              Expression<Func<TEntity, bool>>? filter = null,
              Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null,
              Expression<Func<TEntity, TDto>>? select = null,
              bool isTrackingActive = false,
              CancellationToken cancellationToken = default)
                {
                    IQueryable<TEntity> query = _Entity;

                    if (!isTrackingActive)
                        query = query.AsNoTracking();

                    if (filter != null)
                        query = query.Where(filter);

                    if (includeFunc != null)
                        query = includeFunc(query);

                    if (select != null)
                        return await query.Select(select).ToListAsync(cancellationToken);

                    var entityList = await query.ToListAsync(cancellationToken);
                    return entityList.Cast<TDto>().ToList();
        }



        public async Task<List<TEntity>> GetListAsync(
               Expression<Func<TEntity, bool>>? filter = null,
               Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null,
               bool isTrackingActive = false,
               CancellationToken cancellationToken = default)
                {
                    IQueryable<TEntity> query = _Entity;

                    if (!isTrackingActive)
                        query = query.AsNoTracking();

                    if (filter != null)
                        query = query.Where(filter);

                    if (includeFunc != null)
                        query = includeFunc(query);

                    return await query.ToListAsync(cancellationToken);
                }


        public IQueryable<TEntity> GetQueryByExpression(bool isTraclinActive = false, Expression<Func<TEntity, bool>> expression = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _Entity.AsQueryable();

            if (!isTraclinActive)
            {
                query = query.AsNoTracking();
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
