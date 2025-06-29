using GenericRepository.Options;
using GenericRepository.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GenericRepository
{
    public static class DependencyInjections
    {

        //// Default options for the Repository Service
        //public static IServiceCollection AddGenericRepository(this IServiceCollection services)
        //{
        //    var defaultOptionModel = RepositoryConfigureOptions.CreateDefault();
        //    return services.AddGenericRepository(defaultOptionModel);
        //}

        // user can their own options an action delegate
        public static IServiceCollection AddGenericRepository(this IServiceCollection services, Action<RepositoryConfigureOptions> configureOptions)
        {
            var options = new RepositoryConfigureOptions();
            if (options.Assemblies.Count == 0)
            {
                options.RegisterServicesFromAssembly(Assembly.GetCallingAssembly());
            }
            configureOptions(options);
            return services.AddGenericRepository(options);
        }


        // User can provide their own options
        private static IServiceCollection AddGenericRepository(this IServiceCollection services, RepositoryConfigureOptions optionModel)
        {
            services.AddSingleton(optionModel);
            RegisterRepositories(services, optionModel);
            return services;
        }

        private static void RegisterRepositories(IServiceCollection services, RepositoryConfigureOptions options)
        {
            // Tüm assembly'lerden sadece interface'leri ve Repository türevlerini seç
            var allTypes = options.Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (
                        (t.IsInterface && !t.IsGenericType &&
                            t.GetInterfaces().Any(i =>
                                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>))
                        )
                        ||
                        (t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && IsDerivedFromGeneric(t, typeof(Repository<,>)))
                    )
                ).ToList();



            // IRepository<> den türeyen interface'ler
            var repositoryInterfaces = allTypes
                .Where(t => t.IsInterface)
                .ToList();

            // Repository<,> türevleri
            var repositoryImplementations = allTypes
                .Where(t => t.IsClass)
                .ToList();



            foreach (var repositoryInterface in repositoryInterfaces)
            {
                var implementation = repositoryImplementations.FirstOrDefault(t => t.GetInterfaces().Any(i => i == repositoryInterface));

                if (implementation is not null)
                {
                    services.AddScoped(repositoryInterface, implementation);
                }
                else
                {
                    throw new Exception($"There is no implementation for {repositoryInterface.Name}");
                }
            }

            // Register UnitOfWork
            var unitOfWorkType = typeof(UnitOfWork<>).MakeGenericType(options.DbContextType);
            services.AddScoped(typeof(IUnitOfWork), unitOfWorkType);


        }
        // Helper for checking if a type is derived from a generic type
        private static bool IsDerivedFromGeneric(Type type, Type genericBaseType)
        {
            while (type != null && type != typeof(object))
            {
                var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (currentType == genericBaseType)
                    return true;

                type = type.BaseType;
            }
            return false;
        }
    }

}
