using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository
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
        IQueryable<TEntity> GetAll(bool isTrackingActive = true, Expression<Func<TEntity, bool>> expression = null);
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        TEntity First(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true);
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, bool isTrackingActive = true);
        Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true, CancellationToken cancellationToken = default);
        Task<TEntity> GetFirstAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        bool Any(Expression<Func<TEntity, bool>> expression);
        TEntity GetByExpression(Expression<Func<TEntity, bool>> expression, bool isTrackingActive = true);
        TEntity GetFirst();
        #endregion
        IQueryable<KeyValuePair<bool, int>> CountBy(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    }
}
