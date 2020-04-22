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

using Castle.MicroKernel.Facilities;
using Castle.Windsor;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Contributors;
using PPWCode.Host.Core.Bootstrap.Extensions;

namespace PPWCode.Host.Core.Bootstrap.Facilities
{
    public class AspNetCoreFacility : AbstractFacility
    {
        internal const string IsCrossWiredIntoServiceCollectionKey =
            "windsor-registration-is-also-registered-in-service-collection";

        internal const string IsRegisteredAsMiddlewareIntoApplicationBuilderKey =
            "windsor-registration-is-also-registered-as-middleware";

        private CrossWiringComponentModelContributor _crossWiringComponentModelContributor;
        private MiddlewareComponentModelContributor _middlewareComponentModelContributor;

        protected override void Init()
        {
            Kernel
                .ComponentModelBuilder
                .AddContributor(
                    _crossWiringComponentModelContributor
                    ?? throw new InvalidOperationException(
                        "Please call `Container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));` first. " +
                        "This should happen before any cross wiring registration. " +
                        "Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md"));
        }

        /// <summary>
        ///     Installation of the <see cref="CrossWiringComponentModelContributor" /> for registering components in both
        ///     the <see cref="IWindsorContainer" /> and the <see cref="IServiceCollection" /> via the
        ///     <see cref="WindsorRegistrationExtensions.CrossWired" /> component registration extension
        /// </summary>
        /// <param name="services">
        ///     <see cref="IServiceCollection" />
        /// </param>
        public void CrossWiresInto([NotNull] IServiceCollection services)
            => _crossWiringComponentModelContributor = new CrossWiringComponentModelContributor(services);

        /// <summary>
        ///     Registers Windsor `aware` <see cref="IMiddleware" /> into the <see cref="IApplicationBuilder" /> via the
        ///     <see cref="WindsorRegistrationExtensions.AsMiddleware" /> component registration extension
        /// </summary>
        /// <param name="applicationBuilder">
        ///     <see cref="IApplicationBuilder" />
        /// </param>
        public void RegistersMiddlewareInto([NotNull] IApplicationBuilder applicationBuilder)
        {
            _middlewareComponentModelContributor =
                new MiddlewareComponentModelContributor(_crossWiringComponentModelContributor.Services,
                                                        applicationBuilder);
            Kernel
                .ComponentModelBuilder
                .AddContributor(
                    _middlewareComponentModelContributor); // Happens after Init() in Startup.Configure(IApplicationBuilder, ...)
        }
    }
}
