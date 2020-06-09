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
using System.IO;
using System.Linq;
using System.Reflection;

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Conversion;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Proxy;

using JetBrains.Annotations;

using PPWCode.Host.Core.Bootstrap.Resolvers;

namespace PPWCode.Host.Core.Bootstrap.Facilities
{
    public class ContainerWrapper
    {
        private static ContainerWrapper _current;

        private ContainerWrapper()
        {
            WindsorContainer =
                new WindsorContainer(
                    new DefaultKernel(
                        new InlineDependenciesPropagatingDependencyResolver(),
                        new DefaultProxyFactory()),
                    new DefaultComponentInstaller());
        }

        [NotNull]
        public static ContainerWrapper Current
        {
            get
            {
                if (_current == null)
                {
                    if (_current == null)
                    {
                        _current = new ContainerWrapper();
                    }
                }

                return _current;
            }
        }

        [NotNull]
        public IWindsorContainer WindsorContainer { get; }

        public void Initialise(
            [NotNull] string dllPrefix,
            [CanBeNull] Action<IWindsorContainer> onSetupContainer,
            [CanBeNull] Func<IWindsorContainer, InstallerFactory> onBeforeInstall,
            [CanBeNull] Action<IWindsorContainer> onAfterInstall)
        {
            onSetupContainer?.Invoke(WindsorContainer);

            InstallerFactory installerFactory = onBeforeInstall?.Invoke(WindsorContainer);
            IWindsorInstaller[] installers = GetAssemblies(dllPrefix, installerFactory);
            WindsorContainer.Install(installers);

            IHandlerSelector[] handlerSelectors = WindsorContainer.ResolveAll<IHandlerSelector>();
            foreach (IHandlerSelector handlerSelector in handlerSelectors)
            {
                WindsorContainer
                    .Kernel
                    .AddHandlerSelector(handlerSelector);
            }

            ITypeConverter[] typeConverters = WindsorContainer.ResolveAll<ITypeConverter>();
            IConversionManager conversionManager = WindsorContainer.Kernel.GetConversionManager();
            foreach (ITypeConverter converter in typeConverters)
            {
                conversionManager.Add(converter);
                WindsorContainer.Release(converter);
            }

            onAfterInstall?.Invoke(WindsorContainer);
        }

        [NotNull]
        protected IWindsorInstaller[] GetAssemblies(
            [NotNull] string dllPrefix,
            [CanBeNull] InstallerFactory installerFactory)
        {
            ISet<Assembly> assemblies = new HashSet<Assembly>();
            Assembly root = Assembly.GetExecutingAssembly();

            IEnumerable<Assembly> candidateAssemblies =
                AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(a => (a.FullName != null)
                                && a.FullName.StartsWith(dllPrefix, StringComparison.OrdinalIgnoreCase));
            foreach (Assembly assembly in candidateAssemblies)
            {
                assemblies.Add(assembly);
            }

            string directoryName = Path.GetDirectoryName(root.Location);
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                IEnumerable<string> candidateDirectories =
                    Directory
                        .EnumerateFiles(directoryName)
                        .Where(x => (x != null)
                                    && Path.HasExtension(x)
                                    && string.Equals(Path.GetExtension(x), ".dll",
                                                     StringComparison.InvariantCultureIgnoreCase)
                                    && Path.GetFileName(x).StartsWith(dllPrefix, StringComparison.OrdinalIgnoreCase));
                foreach (string file in candidateDirectories)
                {
                    assemblies.Add(Assembly.LoadFrom(file));
                }
            }

            return
                assemblies
                    .Select(a => FromAssembly.Instance(a, installerFactory ?? new InstallerFactory()))
                    .ToArray();
        }
    }
}
