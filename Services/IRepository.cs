using GenericRepository.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;


namespace GenericRepository.Services
{
    public interface IRepository<TEntity> where TEntity : class
    {
        #region Add
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        void Add(TEntity entity);
        Task AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
        void AddRange(ICollection<TEntity> entities);
        #endregion

        #region Update 
        void Update(TEntity entity);
        void UpdateRange(ICollection<TEntity> entities);
        #endregion

        #region Delete
        void Delete(TEntity entity);
        void DeleteRange(ICollection<TEntity> entities);

        #endregion

        #region Get
        IQueryable<TEntity> Query(
           Expression<Func<TEntity, bool>>? filter = null,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
           bool isTrackingActive = false);

        Task<List<TEntity>> GetListAsync(
                 Expression<Func<TEntity, bool>> filter = null,
                 Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
                 CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetListAsyncNoTracking(
               Expression<Func<TEntity, bool>> filter = null,
               Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
               CancellationToken cancellationToken = default);

        TEntity GetFirstOrDefault(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

         TEntity GetFirstOrDefaultAsNoTracking(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

        Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null
            CancellationToken cancellationToken = default);

        Task<TEntity> GetFirstOrDefaultAsyncNoTracking(
           Expression<Func<TEntity, bool>> expression,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
           CancellationToken cancellationToken = default);


        #endregion

        #region Control
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        bool Any(Expression<Func<TEntity, bool>> expression);
        #endregion

        #region Get Page
        Task<PagingResult<TEntity>> GetPagedAsync(
            PagingRequest request,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool isTrackingActive = false,
            CancellationToken cancellationToken = default);
        #endregion
        IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    }
}
