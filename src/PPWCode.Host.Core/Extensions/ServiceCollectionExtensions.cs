using System;
using System.Linq;

using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Proxy;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace PPWCode.Host.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static T GetSingletonServiceOrNull<T>([NotNull] this IServiceCollection services)
        {
            return (T)services
                .FirstOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }

        public static T GetSingletonService<T>([NotNull] this IServiceCollection services)
        {
            T singletonServiceOrNull = services.GetSingletonServiceOrNull<T>();
            if (singletonServiceOrNull != null)
            {
                return singletonServiceOrNull;
            }

            throw new Exception("Can not find service: " + typeof(T).AssemblyQualifiedName);
        }

        public static IWindsorContainer CreatePPWContainer([CanBeNull] IServiceCollection serviceCollection)
        {
            IWindsorContainer container =
                new WindsorContainer(
                    new DefaultKernel(
                        new InlineDependenciesPropagatingDependencyResolver(),
                        new DefaultProxyFactory()),
                    new DefaultComponentInstaller());
            serviceCollection?.AddSingleton(container);

            return container;
        }
    }
}
