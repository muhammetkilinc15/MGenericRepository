using GenericRepository.Exceptions;
using GenericRepository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GenericRepository.Extensions
{
    public static class QueryableExtensions
    {
        public const int AllRecords = -1;

        public static async Task<PagingResult<T>> ToPagedAsync<T>(
            this IQueryable<T> source,
            PagingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (source is null)
                throw new RepositoryException("Source query cannot be null.");
            if (request is null)
                throw new RepositoryException("PagingRequest cannot be null.");
            if (request.PageNumber < 1)
                throw new RepositoryException("PageNumber must be greater than or equal to 1.");
            if (request.PageSize < 1 && request.PageSize != AllRecords)
                throw new RepositoryException($"PageSize must be greater than or equal to 1, or {AllRecords} to return all records.");

            var totalCount = await source.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.OrderBy))
                source = source.OrderByProperty(request.OrderBy, request.IsDesc);

            IReadOnlyList<T> items;
            if (request.PageSize == AllRecords)
            {
                items = await source.ToListAsync(cancellationToken);
            }
            else
            {
                items = await source
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);
            }

            return new PagingResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize == AllRecords ? totalCount : request.PageSize
            };
        }

        public static async Task<PagingResult<T>> ToPagedAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await source.ToPagedAsync(
                new PagingRequest { PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken);
        }

        public static IOrderedQueryable<T> OrderByProperty<T>(
            this IQueryable<T> source, string propertyPath, bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
                throw new RepositoryException("OrderBy property path cannot be empty.");

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body;
            try
            {
                body = propertyPath.Split('.').Aggregate((Expression)parameter, Expression.Property);
            }
            catch (ArgumentException)
            {
                throw new RepositoryException($"Invalid order-by property '{propertyPath}' for type {typeof(T).Name}.");
            }

            var lambda = Expression.Lambda(body, parameter);
            var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

            var call = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), body.Type },
                source.Expression,
                Expression.Quote(lambda));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }
    }
}
