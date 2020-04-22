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
using System.Linq;

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.LifecycleConcerns;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.ModelBuilder;

using Microsoft.Extensions.DependencyInjection;

using PPWCode.Host.Core.Bootstrap.Facilities;

namespace PPWCode.Host.Core.Bootstrap.Contributors
{
    public class CrossWiringComponentModelContributor : IContributeComponentModelConstruction
    {
        private readonly IServiceCollection _services;

        public CrossWiringComponentModelContributor(IServiceCollection services)
        {
            _services =
                services
                ?? throw new InvalidOperationException(
                    "Please call `Container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));` first." +
                    " This should happen before any cross wiring registration." +
                    " Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md");
        }

        public IServiceCollection Services
            => _services;

        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            if (model.Configuration.Attributes.Get(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey) == bool.TrueString)
            {
                if (model.Lifecycle.HasDecommissionConcerns)
                {
                    DisposalConcern disposableConcern = model.Lifecycle.DecommissionConcerns.OfType<DisposalConcern>().FirstOrDefault();
                    if (disposableConcern != null)
                    {
                        model.Lifecycle.Remove(disposableConcern);
                    }
                }

                foreach (Type serviceType in model.Services)
                {
                    if (model.LifestyleType == LifestyleType.Transient)
                    {
                        _services.AddTransient(serviceType, p => kernel.Resolve(serviceType));
                    }
                    else if (model.LifestyleType == LifestyleType.Scoped)
                    {
                        _services.AddScoped(serviceType, p =>
                                                         {
                                                             kernel.RequireScope();
                                                             return kernel.Resolve(serviceType);
                                                         });
                    }
                    else if (model.LifestyleType == LifestyleType.Singleton)
                    {
                        _services.AddSingleton(serviceType, p => kernel.Resolve(serviceType));
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "The Castle Windsor ASP.NET Core facility only supports the following lifestyles:" +
                            $" {nameof(LifestyleType.Transient)}, {nameof(LifestyleType.Scoped)}" +
                            $" and {nameof(LifestyleType.Singleton)}.");
                    }
                }
            }
        }
    }
}
