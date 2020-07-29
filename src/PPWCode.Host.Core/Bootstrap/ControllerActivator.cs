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
using Castle.Windsor;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace PPWCode.Host.Core.Bootstrap
{
    /// <inheritdoc />
    public class ControllerActivator : IControllerActivator
    {
        public ControllerActivator([NotNull] IWindsorContainer container)
        {
            Kernel = container.Kernel;
        }

        [NotNull]
        public IKernel Kernel { get; }

        /// <inheritdoc />
        public object Create(ControllerContext context)
        {
            IDisposable scope = Kernel.BeginScope();
            context.HttpContext.Response.RegisterForDispose(scope);
            Arguments arguments = new Arguments().AddNamed("controllerContext", context);
            return Kernel.Resolve(context.ActionDescriptor.ControllerTypeInfo.AsType(), arguments);
        }

        /// <inheritdoc />
        public void Release(ControllerContext context, object controller)
            => Kernel.ReleaseComponent(controller);
    }
}