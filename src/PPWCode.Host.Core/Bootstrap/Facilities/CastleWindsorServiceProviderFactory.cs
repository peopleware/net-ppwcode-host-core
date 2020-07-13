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

using Castle.Windsor;
using Castle.Windsor.Installer;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Extensions;
using PPWCode.Host.Core.Bootstrap.Resolvers;

namespace PPWCode.Host.Core.Bootstrap.Facilities
{
    public class CastleWindsorServiceProviderFactory
        : IServiceProviderFactory<IContainerBuilder>
    {
        private readonly object _locker = new object();
        private volatile PPWContainerBuilder _containerBuilder;
        private volatile ServiceProvider _serviceProvider;

        public CastleWindsorServiceProviderFactory(
            [NotNull] ContainerWrapper containerWrapper,
            [NotNull] string dllPrefix,
            [CanBeNull] Action<IWindsorContainer> onSetupContainer,
            [CanBeNull] Func<IWindsorContainer, InstallerFactory> onBeforeInstall,
            [CanBeNull] Action<IWindsorContainer> onAfterInstall)
        {
            ContainerWrapper = containerWrapper;
            DllPrefix = dllPrefix;
            OnSetupContainer = onSetupContainer;
            OnBeforeInstall = onBeforeInstall;
            OnAfterInstall = onAfterInstall;
        }

        [NotNull]
        public ContainerWrapper ContainerWrapper { get; }

        [NotNull]
        public string DllPrefix { get; }

        [CanBeNull]
        public Action<IWindsorContainer> OnSetupContainer { get; }

        [CanBeNull]
        public Func<IWindsorContainer, InstallerFactory> OnBeforeInstall { get; }

        [CanBeNull]
        public Action<IWindsorContainer> OnAfterInstall { get; }

        public IContainerBuilder CreateBuilder([NotNull] IServiceCollection services)
        {
            if (_containerBuilder == null)
            {
                lock (_locker)
                {
                    if (_containerBuilder == null)
                    {
                        // install custom controller-activator + request-scoping-middleware
                        services.AddWindsorServices(ContainerWrapper.WindsorContainer);

                        // Setup the container
                        ContainerWrapper
                            .Initialise(
                                DllPrefix,
                                container =>
                                {
                                    container
                                        .AddSubResolverConditionally(c => new PPWCodeCollectionResolver(c.Kernel, true))
                                        .AddFacilityConditionally<AspNetCoreFacility>(f => f.CrossWiresInto(services));
                                    OnSetupContainer?.Invoke(container);
                                },
                                OnBeforeInstall,
                                OnAfterInstall);

                        _containerBuilder = new PPWContainerBuilder(ContainerWrapper, services);
                    }
                }
            }

            return _containerBuilder;
        }

        [NotNull]
        public IServiceProvider CreateServiceProvider(IContainerBuilder containerBuilder)
        {
            if (_serviceProvider == null)
            {
                lock (_locker)
                {
                    if (_serviceProvider == null)
                    {
                        // ensure that all registrations in IServiceCollection are done,
                        // before building the service-provider!
                        _serviceProvider = containerBuilder.ServiceCollection.BuildServiceProvider();

                        // finalize castle windsor integration
                        // important for cross-wiring: passing the IServiceProvider
                        containerBuilder.ServiceCollection.AddWindsorIntegration(containerBuilder.Container.WindsorContainer, () => _serviceProvider);
                    }
                }
            }

            return _serviceProvider;
        }
    }
}
