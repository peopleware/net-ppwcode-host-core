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

using System.Threading.Tasks;

using Castle.MicroKernel;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Filters;

namespace PPWCode.Host.Core.Bootstrap.ActionFilters
{
    public sealed class ActionFilterProxy<TActionFilter>
        : IAsyncActionFilter,
          IOrderedFilter
        where TActionFilter : class, IAsyncActionFilter, IOrderedFilter
    {
        public ActionFilterProxy(
            [NotNull] IKernel kernel,
            int order)
        {
            Kernel = kernel;
            Order = order;
        }

        [NotNull]
        public IKernel Kernel { get; }

        public int Order { get; }

        /// <inheritdoc />
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            => Kernel.Resolve<TActionFilter>(Arguments.FromProperties(new { order = Order })).OnActionExecutionAsync(context, next);
    }
}
