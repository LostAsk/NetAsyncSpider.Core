using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;


namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionCommonExtensions
    {
        internal static bool IsAdded<T>(this IServiceCollection services)
        {
            return services.IsAdded(typeof(T));
        }

        internal static bool IsAdded(this IServiceCollection services, Type type)
        {
            return services.Any(d => d.ServiceType == type);
        }

        internal static T GetSingletonInstanceOrNull<T>(this IServiceCollection services)
        {
            return (T)services
                .FirstOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }

        internal static T GetSingletonInstance<T>(this IServiceCollection services)
        {
            var service = services.GetSingletonInstanceOrNull<T>();
            if (service == null)
            {
                throw new InvalidOperationException("找不到实例服务: " + typeof(T).AssemblyQualifiedName);
            }

            return service;
        }


    }
}