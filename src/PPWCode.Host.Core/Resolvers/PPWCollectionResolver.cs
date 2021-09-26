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

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;

using JetBrains.Annotations;

namespace PPWCode.Host.Core.Resolvers
{
    public class PPWCollectionResolver : CollectionResolver
    {
        public PPWCollectionResolver(
            [NotNull] IKernel kernel,
            bool allowEmptyCollections = true)
            : base(kernel, allowEmptyCollections)
        {
        }

        public override bool CanResolve(
            [NotNull] CreationContext context,
            [NotNull] ISubDependencyResolver contextHandlerResolver,
            [NotNull] ComponentModel model,
            [NotNull] DependencyModel dependency)
        {
            if (kernel.HasComponent(dependency.TargetItemType))
            {
                return false;
            }

            return base.CanResolve(context, contextHandlerResolver, model, dependency);
        }
    }
}
