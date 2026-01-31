// Copyright 2026 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        public static IWindsorContainer CreatePPWContainer([CanBeNull] this IServiceCollection serviceCollection)
        {
            IWindsorContainer container =
                new WindsorContainer(
                    new DefaultKernel(
                        new InlineDependenciesPropagatingDependencyResolver(),
                        new DefaultProxyFactory()),
                    new DefaultComponentInstaller());
            serviceCollection?.AddSingleton(container);
            serviceCollection?.AddSingleton<IServiceProviderIsService, PPWServiceProviderIsService>();
            return container;
        }
    }
}
