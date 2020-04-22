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

using JetBrains.Annotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Activators;

namespace PPWCode.Host.Core.Bootstrap.Extensions
{
    internal static class AspNetCoreExtensions
    {
        public static void AddCustomControllerActivation(
            [NotNull] this IServiceCollection services,
            [NotNull] Func<ControllerContext, object> resolve,
            [NotNull] Action<ControllerContext, object> release)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (resolve == null)
            {
                throw new ArgumentNullException(nameof(resolve));
            }

            if (release == null)
            {
                throw new ArgumentNullException(nameof(resolve));
            }

            services.AddSingleton<IControllerActivator>(new DelegatingControllerActivator(resolve, release));
        }

        public static void AddRequestScopingMiddleware(
            [NotNull] this IServiceCollection services,
            [NotNull] Func<IEnumerable<IDisposable>> requestScopeProvider)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (requestScopeProvider == null)
            {
                throw new ArgumentNullException(nameof(requestScopeProvider));
            }

            services.AddSingleton<IStartupFilter>(new RequestScopingStartupFilter(requestScopeProvider));
        }

        private sealed class RequestScopingStartupFilter : IStartupFilter
        {
            private readonly Func<IEnumerable<IDisposable>> _requestScopeProvider;

            public RequestScopingStartupFilter([NotNull] Func<IEnumerable<IDisposable>> requestScopeProvider)
            {
                _requestScopeProvider =
                    requestScopeProvider
                    ?? throw new ArgumentNullException(nameof(requestScopeProvider));
            }

            [NotNull]
            public Action<IApplicationBuilder> Configure([NotNull] Action<IApplicationBuilder> nextFilter)
                => builder =>
                   {
                       ConfigureRequestScoping(builder);

                       nextFilter(builder);
                   };

            private void ConfigureRequestScoping([NotNull] IApplicationBuilder builder)
                => builder
                    .Use(async (context, next) =>
                         {
                             IEnumerable<IDisposable> scopes = _requestScopeProvider();
                             try
                             {
                                 await next().ConfigureAwait(false);
                             }
                             finally
                             {
                                 foreach (IDisposable scope in scopes)
                                 {
                                     scope.Dispose();
                                 }
                             }
                         });
        }
    }
}
