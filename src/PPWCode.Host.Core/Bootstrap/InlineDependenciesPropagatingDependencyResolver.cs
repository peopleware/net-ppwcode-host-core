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

using Castle.MicroKernel.Context;
using Castle.MicroKernel.Resolvers;

using JetBrains.Annotations;

namespace PPWCode.Host.Core.Bootstrap
{
    /// <inheritdoc />
    /// <remarks>
    ///     <para> When creating a <see cref="CreationContext" />, use <c>true</c> for last parameter:</para>
    ///     <para>
    ///         When set to <c>true</c> the parent parentContext will be cloned
    ///         <see cref="P:Castle.MicroKernel.Context.CreationContext.AdditionalArguments" />
    ///     </para>
    /// </remarks>
    public class InlineDependenciesPropagatingDependencyResolver : DefaultDependencyResolver
    {
        [NotNull]
        protected override CreationContext RebuildContextForParameter(
            [NotNull] CreationContext current,
            [NotNull] Type parameterType)
            => parameterType.ContainsGenericParameters
                   ? current
                   : new CreationContext(parameterType, current, true);
    }
}
