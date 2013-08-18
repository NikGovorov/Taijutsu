// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;

using Autofac;

using Taijutsu.Event;
using Taijutsu.Event.Internal;

namespace Taijutsu.Sample
{
    public delegate IEnumerable<Func<object>> EventHandlersResolver(Type type);

    public class AutofacFixture
    {
        public void Run()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(GetType().Assembly).AsClosedTypesOf(typeof(IEventHandler<>));

            builder.Register<EventHandlersResolver>(
                config =>
                {
                    var context = config.Resolve<IComponentContext>();
                    return (type) =>
                    {
                        var t1 = typeof(IEventHandler<>).MakeGenericType(type);
                        var t2 = typeof(Func<>).MakeGenericType(t1);
                        var t3 = typeof(IEnumerable<>).MakeGenericType(t2);

                        var resolvers = (IEnumerable<Func<object>>)context.Resolve(t3);
                        return resolvers;
                    };
                });

            var container = builder.Build();

            Events.Subscribe(new DynamicHandlerSettings(typeof(SystemStarted), () => container.Resolve<EventHandlersResolver>()(typeof(SystemStarted)), 0));
            Events.Subscribe(new DynamicHandlerSettings(typeof(SubsystemStarted), () => container.Resolve<EventHandlersResolver>()(typeof(SubsystemStarted)), 0));

            Events.Publish(new SubsystemStarted());
        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }
    }
}