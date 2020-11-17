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

using Castle.Windsor;
using Castle.Windsor.Extensions.DependencyInjection;
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Castle.Windsor.Extensions.DependencyInjection.Resolvers;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Extensions;
using PPWCode.Host.Core.Resolvers;

namespace PPWCode.Host.Core
{
    public class PPWWindsorServiceProviderFactory
        : WindsorServiceProviderFactoryBase
    {
        public PPWWindsorServiceProviderFactory([CanBeNull] IWindsorContainer container)
        {
            SetRootContainer(container);
            CreateRootScope();
        }

        protected override void SetRootContainer([CanBeNull] IWindsorContainer container)
            => rootContainer = container;

        [NotNull]
        protected override IWindsorContainer BuildContainer(
            [CanBeNull] IServiceCollection serviceCollection,
            [CanBeNull] IWindsorContainer container)
        {
            if (container == null)
            {
                container = serviceCollection?.GetSingletonServiceOrNull<IWindsorContainer>();
                if (container != null)
                {
                    SetRootContainer(container);
                }
                else
                {
                    CreateRootContainer();
                    container = Container;
                }
            }

            AddSubSystemToContainer(container);

            RegisterContainer(container);
            RegisterProviders(container);
            RegisterFactories(container);

            AddSubResolvers(container);

            RegisterServiceCollection(serviceCollection, container);

            return container;
        }

        protected override void RegisterServiceCollection(
            [CanBeNull] IServiceCollection serviceCollection,
            [NotNull] IWindsorContainer container)
        {
            if (serviceCollection != null)
            {
                foreach (ServiceDescriptor service in serviceCollection)
                {
                    if (service.ImplementationInstance == container)
                    {
                        continue;
                    }

                    container.Register(service.CreateWindsorRegistration());
                }
            }
        }

        protected virtual void AddSubResolvers([NotNull] IWindsorContainer container)
        {
            container.Kernel.Resolver.AddSubResolver(new PPWCollectionResolver(container.Kernel));
            container.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(container.Kernel));
            container.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(container.Kernel));
        }
    }
}
