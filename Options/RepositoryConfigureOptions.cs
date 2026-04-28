using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace GenericRepository.Options
{
    public sealed class RepositoryConfigureOptions<TContext> where TContext : DbContext
    {
        internal List<Assembly> Assemblies { get; } = new();
        internal Type DbContextType => typeof(TContext);

        public void RegisterServicesFromAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
        }

        public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);
        }
    }
}
