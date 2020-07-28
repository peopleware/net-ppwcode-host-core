// Copyright 2020 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Castle.MicroKernel;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;

using JetBrains.Annotations;

namespace PPWCode.Host.Core.Bootstrap
{
    public static class WindsorContainerExtensions
    {
        public static IKernel AddFacilityConditionally<T>([NotNull] this IKernel kernel, [CanBeNull] Action<T> action = null)
            where T : IFacility, new()
        {
            if (!kernel.GetFacilities().OfType<T>().Any())
            {
                kernel.AddFacility(action);
            }

            return kernel;
        }

        public static IWindsorContainer AddFacilityConditionally<T>([NotNull] this IWindsorContainer container, [CanBeNull] Action<T> action = null)
            where T : IFacility, new()
        {
            container.Kernel.AddFacilityConditionally(action);
            return container;
        }

        private static void AddSubResolverConditionally<T>([NotNull] IKernel kernel, T subDependencyResolver)
            where T : ISubDependencyResolver
        {
            IDependencyResolver currentResolver = kernel.Resolver;
            if (currentResolver is DefaultDependencyResolver
                && typeof(DefaultDependencyResolver)
                    .GetField("subResolvers", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(currentResolver) is IList<ISubDependencyResolver> subResolvers
                && subResolvers.OfType<T>().Any())
            {
                return;
            }

            currentResolver.AddSubResolver(subDependencyResolver);
        }

        public static IKernel AddSubResolverConditionally<T>([NotNull] this IKernel kernel, [NotNull] Func<IKernel, T> func)
            where T : ISubDependencyResolver
        {
            AddSubResolverConditionally(kernel, func(kernel));
            return kernel;
        }

        public static IWindsorContainer AddSubResolverConditionally<T>([NotNull] this IWindsorContainer container, [NotNull] Func<IWindsorContainer, T> func)
            where T : ISubDependencyResolver
        {
            AddSubResolverConditionally(container.Kernel, func(container));
            return container;
        }
    }
}
