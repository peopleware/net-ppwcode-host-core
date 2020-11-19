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

using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Proxy;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap;

namespace PPWCode.Host.Core.Extensions
{
    public static class WindsorExtensions
    {
        public static IWindsorContainer CreatePPWContainer([CanBeNull] IServiceCollection serviceCollection)
        {
            IWindsorContainer container =
                new WindsorContainer(
                    new DefaultKernel(
                        new InlineDependenciesPropagatingDependencyResolver(),
                        new DefaultProxyFactory()),
                    new DefaultComponentInstaller());

            if (serviceCollection != null)
            {
                serviceCollection
                    .AddSingleton(container)
                    .AddSingleton<IControllerActivator>(new ControllerActivator(container));
            }

            return container;
        }
    }
}
