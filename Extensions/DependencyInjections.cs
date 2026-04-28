using GenericRepository.Options;
using GenericRepository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GenericRepository
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddGenericRepository<TContext>(
            this IServiceCollection services,
            Action<RepositoryConfigureOptions<TContext>> configureOptions)
            where TContext : DbContext
        {
            var options = new RepositoryConfigureOptions<TContext>();
            configureOptions(options);

            if (options.Assemblies.Count == 0)
                options.RegisterServicesFromAssembly(Assembly.GetCallingAssembly());

            RegisterRepositories<TContext>(services, options);
            RegisterUnitOfWork<TContext>(services, options);

            return services;
        }

        private static void RegisterRepositories<TContext>(
            IServiceCollection services,
            RepositoryConfigureOptions<TContext> options)
            where TContext : DbContext
        {
            var allTypes = options.Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (t.IsInterface && !t.IsGenericType &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>)))
                    ||
                    (t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition &&
                        IsDerivedFromGeneric(t, typeof(Repository<,>))))
                .ToList();

            var repositoryInterfaces = allTypes.Where(t => t.IsInterface).ToList();
            var repositoryImplementations = allTypes.Where(t => t.IsClass).ToList();

            foreach (var repositoryInterface in repositoryInterfaces)
            {
                var implementation = repositoryImplementations.FirstOrDefault(t =>
                    t.GetInterfaces().Any(i => i == repositoryInterface) &&
                    UsesContext(t, typeof(TContext)));

                if (implementation is null)
                    continue;

                services.AddScoped(repositoryInterface, implementation);
            }
        }

        private static void RegisterUnitOfWork<TContext>(
            IServiceCollection services,
            RepositoryConfigureOptions<TContext> options)
            where TContext : DbContext
        {
            var concreteUow = typeof(UnitOfWork<TContext>);
            var genericUowInterface = typeof(IUnitOfWork<TContext>);

            services.AddScoped(concreteUow);
            services.AddScoped(genericUowInterface, sp => sp.GetRequiredService(concreteUow));

            var markerInterfaces = options.Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsInterface &&
                    !t.IsGenericType &&
                    t != genericUowInterface &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IUnitOfWork<>) &&
                        i.GenericTypeArguments[0] == typeof(TContext)))
                .ToList();

            foreach (var marker in markerInterfaces)
                services.AddScoped(marker, sp => sp.GetRequiredService(concreteUow));
        }

        private static bool UsesContext(Type implementation, Type contextType)
        {
            var current = implementation;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(Repository<,>))
                {
                    return current.GenericTypeArguments[1] == contextType;
                }
                current = current.BaseType;
            }
            return false;
        }

        private static bool IsDerivedFromGeneric(Type type, Type genericBaseType)
        {
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == genericBaseType)
                    return true;

                type = type.BaseType;
            }
            return false;
        }
    }
}
