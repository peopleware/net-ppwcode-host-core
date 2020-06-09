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

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.ModelBuilder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Facilities;

namespace PPWCode.Host.Core.Bootstrap.Contributors
{
    public class MiddlewareComponentModelContributor : IContributeComponentModelConstruction
    {
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly IServiceCollection _services;
        private IServiceProvider _provider;

        public MiddlewareComponentModelContributor(IServiceCollection services, IApplicationBuilder applicationBuilder)
        {
            _services =
                services
                ?? throw new ArgumentNullException(nameof(services));
            _applicationBuilder =
                applicationBuilder
                ?? throw new InvalidOperationException(
                    "Please call `Container.GetFacility<AspNetCoreFacility>(f => f.RegistersMiddlewareInto(applicationBuilder));` first. " +
                    "This should happen before any middleware registration. " +
                    "Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md");
        }

        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            if (model.Configuration.Attributes.Get(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey) == bool.TrueString)
            {
                foreach (Type service in model.Services)
                {
                    _applicationBuilder.Use(
                        async (context, next) =>
                        {
                            using (kernel.BeginScope())
                            {
                                if (_provider == null)
                                {
                                    _provider = _services.BuildServiceProvider();
                                }

                                IServiceScope serviceProviderScope = _provider.CreateScope();
                                try
                                {
                                    IMiddleware middleware = (IMiddleware)kernel.Resolve(service);
                                    await middleware.InvokeAsync(context, ctx => next()).ConfigureAwait(false);
                                }
                                finally
                                {
                                    serviceProviderScope.Dispose();
                                }
                            }
                        });
                }
            }
        }
    }
}
