
![NuGet Downloads](https://img.shields.io/nuget/dt/MGenericRepository.svg)
![NuGet Version](https://img.shields.io/nuget/v/MGenericRepository.svg)
![NuGet Pre-release Version](https://img.shields.io/nuget/vpre/MGenericRepository.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

# Generic Repository for .NET Core

The **Generic Repository** pattern is used to simplify data access operations in .NET Core applications. This package offers support for **CRUD** operations, custom queries, **Unit of Work** for transaction management, **pagination**, **dynamic ordering**, and **multiple DbContext** support.

## Features
- **CRUD operations** with both sync and async support.
- **Custom queries** via LINQ and `Expression` trees.
- **`IIncludableQueryable`-based include API** — supports `ThenInclude`, filtered includes, etc.
- **Pagination** through `PagingRequest` / `PagingResult<T>` and `IQueryable.ToPagedAsync()` extension.
- **Dynamic ordering** with `OrderByProperty("Author.Name")` — supports nested properties.
- **Unit of Work** generic per `DbContext` (`IUnitOfWork<TContext>`) with transaction management.
- **Multiple DbContext support** — register repositories and unit of works per context, no collisions.
- **Marker interfaces** — auto-detected so you can inject `IAppUnitOfWork` instead of `IUnitOfWork<ApplicationDbContext>`.

## Installation

```bash
dotnet add package MGenericRepository
```

---

## 1. Define Your Repository

```csharp
public interface IProductRepository : IRepository<Product>
{
}

public class ProductRepository : Repository<Product, ApplicationDbContext>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
}
```

The `Repository<TEntity, TContext>` base class binds your repository to a specific `DbContext`.

---

## 2. Register Services in `Program.cs`

### Single layer / single project

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddGenericRepository<ApplicationDbContext>(options =>
{
    // Optional — defaults to the calling assembly if omitted
    // options.RegisterServicesFromAssembly(typeof(ProductRepository).Assembly);
});
```

### Clean Architecture / Onion Architecture

In layered solutions, the **interface** lives in the Application layer and the **implementation** lives in the Persistence/Infrastructure layer. Both assemblies must be scanned:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddGenericRepository<ApplicationDbContext>(options =>
{
    var applicationAssembly = typeof(IProductRepository).Assembly;
    var persistenceAssembly = typeof(ProductRepository).Assembly;

    options.RegisterServicesFromAssemblies(applicationAssembly, persistenceAssembly);
});
```

### Multiple DbContexts (modular monolith / CQRS / Identity split)

You can call `AddGenericRepository<TContext>` multiple times, once per context:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(appConn));
builder.Services.AddDbContext<CatalogDbContext>(opt => opt.UseNpgsql(catalogConn));

builder.Services.AddGenericRepository<ApplicationDbContext>(options =>
    options.RegisterServicesFromAssemblies(
        typeof(IProductRepository).Assembly,
        typeof(ProductRepository).Assembly));

builder.Services.AddGenericRepository<CatalogDbContext>(options =>
    options.RegisterServicesFromAssemblies(
        typeof(ICategoryRepository).Assembly,
        typeof(CategoryRepository).Assembly));
```

Each `Repository<TEntity, TContext>` is registered only against the matching context — no collisions.

---

## 3. Inject the Unit of Work

### Single context

Inject `IUnitOfWork<TContext>` directly:

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

    public ProductService(
        IProductRepository repository,
        IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> AddProductAsync(Product product, CancellationToken ct)
    {
        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return product;
    }
}
```

### Multiple contexts — use marker interfaces

Writing `IUnitOfWork<ApplicationDbContext>` everywhere is verbose. Define a marker interface in your Application layer — **no class needed**, the package auto-registers it:

```csharp
// Application layer
public interface IAppUnitOfWork : IUnitOfWork<ApplicationDbContext> { }
public interface ICatalogUnitOfWork : IUnitOfWork<CatalogDbContext> { }
```

That's it. No implementation class to write — the package binds these markers to the correct `UnitOfWork<TContext>` automatically during DI registration.

```csharp
public class OrderService
{
    public OrderService(IAppUnitOfWork appUow, ICatalogUnitOfWork catalogUow) { ... }
}
```

> Make sure the assembly containing your marker interfaces is registered via
> `RegisterServicesFromAssembly(...)` so the scanner can find them.

---

## 4. Querying

### Filter + include + tracking

`Include` uses the `IIncludableQueryable` pattern, so `ThenInclude` works:

```csharp
var product = await _repository.FirstOrDefaultAsync(
    p => p.Id == id,
    include: q => q.Include(p => p.Category)
                   .Include(p => p.Reviews).ThenInclude(r => r.User),
    isTrackingActive: false,
    cancellationToken: ct);
```

### Lists

```csharp
var products = await _repository.GetListAsync(
    filter: p => p.IsActive,
    include: q => q.Include(p => p.Category),
    cancellationToken: ct);
```

### Building your own query with `Query()`

When the built-in helpers aren't enough, use `Query()` to get an `IQueryable<T>` and compose freely:

```csharp
var query = _repository.Query(
    filter: p => p.Price > 100,
    include: q => q.Include(p => p.Category));

var topProducts = await query
    .OrderByDescending(p => p.SalesCount)
    .Take(10)
    .ToListAsync(ct);
```

---

## 5. Pagination

### Through the repository

```csharp
var page = await _repository.GetPagedAsync(
    new PagingRequest
    {
        PageNumber = 1,
        PageSize = 20,
        OrderBy = "CreateAppUser.FullName", // nested property paths supported
        IsDesc = true
    },
    filter: p => p.IsActive,
    include: q => q.Include(p => p.CreateAppUser),
    cancellationToken: ct);

// page.Items, page.TotalCount, page.PageNumber, page.PageSize
// page.TotalPages, page.HasPrevious, page.HasNext
```

### On any `IQueryable<T>` via `ToPagedAsync()`

If you already have a custom query, paginate it with the extension:

```csharp
using GenericRepository.Extensions;

var page = await _context.Products
    .AsNoTracking()
    .Where(p => p.IsActive)
    .Include(p => p.Category)
    .ToPagedAsync(new PagingRequest
    {
        PageNumber = 1,
        PageSize = 20,
        OrderBy = "Name"
    }, ct);
```

### Return all records

Pass `PageSize = QueryableExtensions.AllRecords` (or `-1`) to skip paging and return everything:

```csharp
var all = await _repository.GetPagedAsync(
    new PagingRequest
    {
        PageNumber = 1,
        PageSize = QueryableExtensions.AllRecords
    });
```

### Dynamic ordering on its own

```csharp
using GenericRepository.Extensions;

var ordered = _context.Products
    .OrderByProperty("Category.Name", descending: true);
```

---

## 6. Transactions

```csharp
public async Task TransferAsync(Guid fromId, Guid toId, decimal amount, CancellationToken ct)
{
    await _unitOfWork.BeginTransactionAsync(ct);
    try
    {
        // ... your work ...
        await _unitOfWork.CommitAsync(ct);
    }
    catch
    {
        await _unitOfWork.RollbackAsync(ct);
        throw;
    }
}
```

---

## 7. Bulk Operations

Insert in batches with `AddRangeAsync`:

```csharp
await _repository.AddRangeAsync(products, ct);
await _unitOfWork.SaveChangesAsync(ct);
```

For bulk update / delete in a single round-trip, use EF Core's native
`ExecuteUpdateAsync` / `ExecuteDeleteAsync` directly on `Query()`:

```csharp
await _repository.Query(p => p.IsDiscontinued)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false), ct);

await _repository.Query(p => p.IsDiscontinued)
    .ExecuteDeleteAsync(ct);
```

---

## API Quick Reference

| Method | Purpose |
| --- | --- |
| `AddAsync` / `Add` / `AddRangeAsync` | Insert entities |
| `Update` / `UpdateRange` | Update entities |
| `Delete` / `DeleteRange` | Remove entities |
| `FirstOrDefault` / `FirstOrDefaultAsync` | Single entity by predicate |
| `GetListAsync` | List of entities by predicate |
| `Query` | Compose your own `IQueryable<T>` |
| `GetPagedAsync` | Paginated result |
| `Any` / `AnyAsync` / `CountBy` | Existence and counting |
| `IUnitOfWork<TContext>` | `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`, `SaveChangesAsync` |

---

## Breaking Changes (v3.0)

- `IUnitOfWork` is now generic: `IUnitOfWork<TContext>`. Update all injection sites.
- `AddGenericRepository(...)` is now generic: `AddGenericRepository<TContext>(...)`. The `UseDbContext<T>()` option is removed — pass the context as a generic parameter instead.
- Include parameters now use `Func<IQueryable<T>, IIncludableQueryable<T, object>>` instead of `params Expression<Func<T, object>>[]`. Rewrite includes as lambdas: `q => q.Include(...).ThenInclude(...)`.
- `GetByExpression` / `GetByExpressionAsync` removed — use `FirstOrDefault` / `FirstOrDefaultAsync` (identical behavior).
- `First` / `FirstAsync` / `GetFirst` / `GetFirstAsync` removed — use `FirstOrDefault` / `FirstOrDefaultAsync` and handle null explicitly, or call `Query().First()`.
- Sync `Where` removed — use `Query(filter: ...)`.
