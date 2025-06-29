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
        Task UpdateByExpressionAsync(Expression<Func<TEntity, bool>> filterExpression,
                        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression,
                        CancellationToken cancellationToken = default);
        #endregion

        #region Delete
        Task DeleteByIdAsync(string id);
        Task DeleteByExpressionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        void Delete(TEntity entity);
        void DeleteRange(ICollection<TEntity> entities);

        #endregion

        #region Get
        IQueryable<TEntity> AsQueryable(bool isTrackingActive = false);
        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default, bool isTrackingActive = false);
        IQueryable<TEntity> GetQueryByExpression(bool isTrackingActive = false, Expression<Func<TEntity, bool>> expression = null, params Expression<Func<TEntity, object>>[] includes);
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        TEntity First(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true);
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true);
        Task<TEntity> GetByExpressionAsync(
                Expression<Func<TEntity, bool>> expression,
                bool isTrackingActive = true,
                CancellationToken cancellationToken = default,
                params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity> GetFirstAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        bool Any(Expression<Func<TEntity, bool>> expression);
        TEntity GetByExpression(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true, params Expression<Func<TEntity, object>>[] includes);
        TEntity GetFirst();
        #endregion
        IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    }
}
