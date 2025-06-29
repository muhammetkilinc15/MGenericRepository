
![NuGet Downloads](https://img.shields.io/nuget/dt/MGenericRepository.svg)
![NuGet Version](https://img.shields.io/nuget/v/MGenericRepository.svg)
![NuGet Pre-release Version](https://img.shields.io/nuget/vpre/MGenericRepository.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

# Generic Repository for .NET Core

The **Generic Repository** pattern is used to simplify data access operations in .NET Core applications. This package offers support for **CRUD** operations, custom queries, and **Unit of Work** for transaction management. It helps keep your code clean, understandable, and maintainable.

## Features:
- **CRUD Operations**: Provides basic Create, Read, Update, and Delete operations for entities.
- **Custom Queries**: Allows you to create custom queries using LINQ and `Expression` trees.
- **Asynchronous Support**: All operations are supported asynchronously using `async/await`.
- **Unit of Work Support**: Integrates Unit of Work for transaction management and consistency.

## Installation

To install the package, use the following command:

```bash
dotnet add package MGenericRepository 
```

#### Create Repository
```csharp
public interface IProductRepository : IRepository<Product>
{
}

 public class ProductRepository : Repository<Product, ApplicationDbContext>, IProductRepository
 {
     public ProductRepository(ApplicationDbContext context) : base(context)
     {
     }
 }

```

----

Usage Examples
```csharp
 public interface IProductService
 {
     Task<List<Product>> GetProducts(int page, int pageSize);
     Task<Product> GetProduct(int id);
     Task<Product> AddProduct(Product product);
     Task<Product> UpdateProduct(Product product);
 }

  public class ProductService : IProductService
  {
      private readonly IProductRepository repository;
      private readonly IUnitOfWork unitOfWork;


      public ProductService(IProductRepository repository, IUnitOfWork unitOfWork)
      {
          this.repository = repository;
          this.unitOfWork = unitOfWork;
      }

      public async Task<List<Product>> GetProducts()
      {
          return await repository.GetAllAsync();
      }
      public async Task<Product> AddProduct(Product product)
      {
          await repository.AddAsync(product);
          await unitOfWork.SaveChangesAsync();
          return product;
      }
}
```

----

#### Implementation in program.cs ####



* 1-) If you are using the repository in a single layer
```csharp
   builder.Services.AddGenericRepository(options =>
    {
        options.UseDbContext<ApplicationDbContext>();
        // options.RegisterServicesFromAssembly(typeof(ProductRepository).Assembly); // Register services from the specified assembly)
        // Assembly will be auto-registered: uses the calling assembly by default
    });

```
* 2-) If you are using Clean Architecture / Onion Architecture
``` csharp
    builder.Services.AddGenericRepository(options =>
    {
        options.UseDbContext<ApplicationDbContext>();

        var persistenceAssembly = typeof(ProductRepository).Assembly;
        var applicationAssembly = typeof(IProductRepository).Assembly;

        options.RegisterServicesFromAssemblies(persistenceAssembly, applicationAssembly);
    });

```