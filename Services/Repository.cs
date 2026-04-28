using GenericRepository.Exceptions;
using GenericRepository.Extensions;
using GenericRepository.Models;
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

        public virtual void UpdateRange(ICollection<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new RepositoryException("No entities provided to update.");

            _Entity.UpdateRange(entities);
        }

        #endregion Update

        #region Control
        public virtual bool Any(Expression<Func<TEntity, bool>> expression)
        {
            return _Entity.Any(expression);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.AnyAsync(expression, cancellationToken);
        }

        #endregion Control

        #region Count

        public virtual IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _Entity.CountBy(expression);
        }
        #endregion Count

        #region Get
        public IQueryable<TEntity> Query(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
           bool isTrackingActive = false)
        {
            return BuildQuery(include, isTrackingActive, filter);
        }

        public async Task<List<TEntity>> GetListAsync(
             Expression<Func<TEntity, bool>> filter = null,
             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
             CancellationToken cancellationToken = default)
        {
            return await BuildQuery(include, true, filter).ToListAsync(cancellationToken);
        }

        public async Task<List<TEntity>> GetListAsyncNoTracking(
               Expression<Func<TEntity, bool>> filter = null,
               Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
               CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _Entity;
            return await BuildQuery(include, false, filter).ToListAsync(cancellationToken);
        }
        public virtual TEntity GetFirstOrDefault(
            Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            return BuildQuery(include, true, filter).FirstOrDefault();
        }

        public virtual TEntity GetFirstOrDefaultAsNoTracking(
         Expression<Func<TEntity, bool>> filter,
         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            return BuildQuery(include, false, filter).FirstOrDefault();
        }
        public virtual async Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            CancellationToken cancellationToken = default)
        {
            return await BuildQuery(include, true, filter).FirstOrDefaultAsync(cancellationToken);
        }
        public virtual async Task<TEntity> GetFirstOrDefaultAsyncNoTracking(
          Expression<Func<TEntity, bool>> expression,
          Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
          CancellationToken cancellationToken = default)
        {
            return await BuildQuery(include, false).FirstOrDefaultAsync(expression, cancellationToken);
        }

        private IQueryable<TEntity> BuildQuery(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            bool isTrackingActive,
            Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _Entity;

            if (!isTrackingActive)
                query = query.AsNoTracking();

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);
            return query;
        }

        #endregion

        #region Get Page
        public virtual Task<PagingResult<TEntity>> GetPagedAsync(
            PagingRequest request,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool isTrackingActive = false,
            CancellationToken cancellationToken = default)
        {
            return Query(filter, include, isTrackingActive).ToPagedAsync(request, cancellationToken);
        }
        #endregion
    }
}
