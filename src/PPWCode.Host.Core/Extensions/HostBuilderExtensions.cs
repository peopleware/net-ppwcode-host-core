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
using Castle.Windsor.Extensions.DependencyInjection;

using JetBrains.Annotations;

using PPWCode.Host.Core;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
		/// <summary>
		/// Uses <see name="IWindsorContainer" /> as the DI container for the host
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <returns>Host builder</returns>
		[NotNull]
		public static IHostBuilder UsePPWWindsorContainerServiceProvider([NotNull] this IHostBuilder hostBuilder)
			=> hostBuilder.UseServiceProviderFactory(new PPWWindsorServiceProviderFactory(null));

		/// <summary>
		/// Uses <see name="IWindsorContainer" /> as the DI container for the host
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="factoryArgs">Ctor arguments for the creation of the factory</param>
		/// <returns>Host builder, creates instance of factory passed as generic</returns>
		[NotNull]
		public static IHostBuilder UseWindsorContainerServiceProvider<T>([NotNull] this IHostBuilder hostBuilder, params object[] factoryArgs)
			where T : WindsorServiceProviderFactoryBase
			=> hostBuilder.UseServiceProviderFactory((T)Activator.CreateInstance(typeof(T), factoryArgs));

		/// <summary>
		/// Uses <see name="IWindsorContainer" /> as the DI container for the host
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="serviceProviderFactory">Instance of WindsorServiceProviderFactoryBase to be used as ServiceProviderFactory</param>
		/// <returns>Host builder</returns>
		[NotNull]
		public static IHostBuilder UseWindsorContainerServiceProvider<T>([NotNull] this IHostBuilder hostBuilder, [NotNull] T serviceProviderFactory)
			where T : WindsorServiceProviderFactoryBase
			=> hostBuilder.UseServiceProviderFactory(serviceProviderFactory);

		/// <summary>
		/// Uses <see name="IWindsorContainer" /> as the DI container for the host
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name = "container">Windsor Container to be used for registrations, please note, will be cleared of all existing registrations</param>
		/// <returns>Host builder</returns>
		[NotNull]
		public static IHostBuilder UsePPWWindsorContainerServiceProvider([NotNull] this IHostBuilder hostBuilder, [CanBeNull] IWindsorContainer container)
			=> hostBuilder.UseServiceProviderFactory(new PPWWindsorServiceProviderFactory(container));
    }
}
