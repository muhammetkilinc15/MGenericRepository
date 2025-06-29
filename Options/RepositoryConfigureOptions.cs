using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace GenericRepository.Options
{
    public sealed class RepositoryConfigureOptions
    {
        internal List<Assembly> Assemblies { get; set; } = [];
        internal Type DbContextType { get; private set; } = typeof(DbContext);

        public void RegisterServicesFromAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
        }
        public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);
        }
        public void UseDbContext<TContext>() where TContext : DbContext
        {
            DbContextType = typeof(TContext);
        }

        public static RepositoryConfigureOptions CreateDefault()
        {
            var options = new RepositoryConfigureOptions();
            options.RegisterServicesFromAssembly(Assembly.GetCallingAssembly());
            return options;
        }
    }

}
