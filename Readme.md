
![NuGet Downloads](https://img.shields.io/nuget/dt/MGenericRepository.svg)
![NuGet Version](https://img.shields.io/nuget/v/MGenericRepository.svg)
![NuGet Pre-release Version](https://img.shields.io/nuget/vpre/MGenericRepository.svg)


# Generic Repository for .NET Core

**Generic Repository** deseni, .NET Core uygulamalarında veri erişim işlemlerini basitleştirmek için kullanılan bir yapıdır. Bu paket, **CRUD** işlemleri, özel sorgular ve **Unit of Work** ile işlem yönetimi desteği sunar. Kodunuzu daha temiz, anlaşılır ve sürdürülebilir hale getirir.

## Özellikler:
- **CRUD İşlemleri**: Varlıklar için temel oluşturma, okuma, güncelleme ve silme işlemleri sağlar.
- **Özel Sorgular**: LINQ ve `Expression` ağaçları ile özel sorgular oluşturmanıza imkan verir.
- **Asenkron Destek**: Tüm işlemler asenkron (`async/await`) olarak desteklenir.
- **Unit of Work Desteği**: İşlem bütünlüğü ve yönetimi için Unit of Work ile entegrasyon.

## Kurulum

Paketin kurulumu için şu komutu kullanabilirsiniz:

```bash
dotnet add package MGenericRepository 
```

#### Create Repository
```csharp
public interface IUserRepository : IRepository<User>
{
}

public class UserRepository : Repository<User, ApplicationDbContext>, IUserRepository
{

}

```

----

#### Unit of Work Implementation

```csharp
  public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

    }

```
#### Dependency Injection
```csharp
  builder.Service.AddScoped<IUserRepository, UserRepository>();
  builder.Services.AddScoped<IUnitOfWork>(srv => srv.GetRequiredService<ApplicationDbContext>());

```

