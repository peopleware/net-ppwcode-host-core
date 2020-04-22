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

using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Facilities;
using PPWCode.Host.Core.Bootstrap.Resolvers;

namespace PPWCode.Host.Core.Bootstrap.Extensions
{
    public static class WindsorRegistrationExtensions
    {
        /// <summary>
        ///     Sets up framework level activators for Controllers and adds additional sub dependency resolvers
        /// </summary>
        /// <param name="services">
        ///     <see cref="IServiceCollection" />
        /// </param>
        /// <param name="container">
        ///     <see cref="IWindsorContainer" />
        /// </param>
        public static void AddWindsorServices(
            [NotNull] this IServiceCollection services,
            [NotNull] IWindsorContainer container)
        {
            InstallFrameworkIntegration(services, container);
        }

        /// <summary>
        ///     Sets up framework level activators for Controllers and adds additional sub dependency resolvers
        /// </summary>
        /// <param name="services">
        ///     <see cref="IServiceCollection" />
        /// </param>
        /// <param name="container">
        ///     <see cref="IWindsorContainer" />
        /// </param>
        /// <param name="serviceProviderFactory">Optional factory for creating a custom <see cref="IServiceProvider" /></param>
        [NotNull]
        public static IServiceProvider AddWindsorIntegration(
            [NotNull] this IServiceCollection services,
            [NotNull] IWindsorContainer container,
            [CanBeNull] Func<IServiceProvider> serviceProviderFactory = null)
        {
            InstallWindsorIntegration(services, container);
            return InitialiseFrameworkServiceProvider(serviceProviderFactory, container);
        }

        /// <summary>
        ///     For making types available to the <see cref="IServiceCollection" /> using 'late bound' factories which resolve from
        ///     Windsor.
        /// </summary>
        /// <param name="registration">
        ///     The component registration that gets copied across to the <see cref="IServiceCollection" />
        /// </param>
        [NotNull]
        public static ComponentRegistration CrossWired([NotNull] this ComponentRegistration registration)
        {
            registration.Attribute(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey).Eq(bool.TrueString);
            return registration;
        }

        /// <summary>
        ///     For making types available to the <see cref="IServiceCollection" /> using 'late bound' factories which resolve from
        ///     Windsor.
        /// </summary>
        /// <param name="registration">The component registration that gets copied across to the IServiceCollection</param>
        [NotNull]
        public static ComponentRegistration<T> CrossWired<T>([NotNull] this ComponentRegistration<T> registration)
            where T : class
        {
            registration.Attribute(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey).Eq(bool.TrueString);
            return registration;
        }

        /// <summary>
        ///     For registering middleware that is resolved from Windsor.
        /// </summary>
        /// <param name="registration">
        ///     <see cref="ComponentRegistration" />
        /// </param>
        /// <returns>
        ///     <see cref="ComponentRegistration" />
        /// </returns>
        [NotNull]
        public static ComponentRegistration AsMiddleware([NotNull] this ComponentRegistration registration)
        {
            registration.Attribute(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey).Eq(bool.TrueString);
            return registration;
        }

        /// <summary>
        ///     For registering middleware that is resolved from Windsor.
        /// </summary>
        /// <typeparam name="T">A generic type that implements <see cref="IMiddleware" /></typeparam>
        /// <param name="registration">
        ///     <see cref="ComponentRegistration{T}" />
        /// </param>
        /// <returns>
        ///     <see cref="ComponentRegistration{T}" />
        /// </returns>
        [NotNull]
        public static ComponentRegistration<T> AsMiddleware<T>([NotNull] this ComponentRegistration<T> registration)
            where T : class, IMiddleware
        {
            registration.Attribute(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey).Eq(bool.TrueString);
            return registration;
        }

        [NotNull]
        private static IServiceProvider InitialiseFrameworkServiceProvider(
            [NotNull] Func<IServiceProvider> serviceProviderFactory,
            [NotNull] IWindsorContainer container)
        {
            IServiceProvider serviceProvider = serviceProviderFactory();

            container
                .Register(
                    Component
                        .For<IServiceProvider>()
                        .Instance(serviceProvider));

            foreach (IAcceptServiceProvider acceptServiceProvider in container.ResolveAll<IAcceptServiceProvider>())
            {
                acceptServiceProvider.AcceptServiceProvider(serviceProvider);
            }

            return serviceProvider;
        }

        private static void InstallFrameworkIntegration(
            [NotNull] IServiceCollection services,
            [NotNull] IWindsorContainer container)
        {
            services
                .AddRequestScopingMiddleware(
                    () => new[]
                          {
                              container.RequireScope(),
                              container.Resolve<IServiceProvider>().CreateScope()
                          });
            services
                .AddCustomControllerActivation(
                    context => container.Resolve(
                        context.ActionDescriptor.ControllerTypeInfo.AsType(),
                        Arguments.FromProperties(new { controllerContext = context })),
                    (context, o) => container.Release(o));
        }

        private static void InstallWindsorIntegration(
            [NotNull] IServiceCollection services,
            [NotNull] IWindsorContainer container)
        {
            LoggerDependencyResolver loggerDependencyResolver = new LoggerDependencyResolver();
            container.Register(Component.For<IAcceptServiceProvider>().Instance(loggerDependencyResolver));
            container.Kernel.Resolver.AddSubResolver(loggerDependencyResolver);

            FrameworkDependencyResolver frameworkDependencyResolver = new FrameworkDependencyResolver(services);
            container.Register(Component.For<IAcceptServiceProvider>().Instance(frameworkDependencyResolver));
            container.Kernel.Resolver.AddSubResolver(frameworkDependencyResolver);
        }
    }
}
