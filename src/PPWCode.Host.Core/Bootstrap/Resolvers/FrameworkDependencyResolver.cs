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
using System.Linq;

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace PPWCode.Host.Core.Bootstrap.Resolvers
{
    public class FrameworkDependencyResolver
        : ISubDependencyResolver,
          IAcceptServiceProvider
    {
        public FrameworkDependencyResolver([NotNull] [ItemNotNull] IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        [NotNull]
        [ItemNotNull]
        public IServiceCollection ServiceCollection { get; }

        [CanBeNull]
        public IServiceProvider ServiceProvider { get; private set; }

        public void AcceptServiceProvider([NotNull] IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider;

        public bool CanResolve(
            [NotNull] CreationContext context,
            [NotNull] ISubDependencyResolver contextHandlerResolver,
            [NotNull] ComponentModel model,
            [NotNull] DependencyModel dependency)
            => HasMatchingType(dependency.TargetType);

        public object Resolve(
            [NotNull] CreationContext context,
            [NotNull] ISubDependencyResolver contextHandlerResolver,
            [NotNull] ComponentModel model,
            [NotNull] DependencyModel dependency)
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException("The serviceProvider for this resolver is null. Please call AcceptServiceProvider first.");
            }

            return ServiceProvider.GetService(dependency.TargetType);
        }

        public bool HasMatchingType(Type dependencyType)
            => ServiceCollection.Any(x => x.ServiceType.MatchesType(dependencyType));
    }

    internal static class GenericTypeExtensions
    {
        public static bool MatchesType(this Type type, Type otherType)
        {
            Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            Type genericOtherType = otherType.IsGenericType ? otherType.GetGenericTypeDefinition() : otherType;
            return (genericType == genericOtherType) || (genericOtherType == genericType);
        }
    }
}
