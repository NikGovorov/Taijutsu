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
using System.ComponentModel;
using System.Linq;

namespace Taijutsu.Event.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypedHandlerSettings<TEvent> : IEventHandlerSettings where TEvent : class, IEvent
    {
        private readonly Type type;

        private readonly Func<ISpecEventHandler<TEvent>> handlerResolver;

        private readonly IEnumerable<Func<TEvent, bool>> filters;

        private readonly int priority;

        public TypedHandlerSettings(Func<ISpecEventHandler<TEvent>> handlerResolver, IEnumerable<Func<TEvent, bool>> filters = null, int priority = 0)
        {
            if (handlerResolver == null)
            {
                throw new ArgumentNullException("handlerResolver");
            }

            type = typeof(TEvent);
            this.handlerResolver = handlerResolver;
            this.filters = filters ?? new List<Func<TEvent, bool>>();
            this.priority = priority;
        }

        int IEventHandlerSettings.Priority
        {
            get { return priority; }
        }

        Type IEventHandlerSettings.Type
        {
            get { return type; }
        }

        Action<object> IEventHandlerSettings.Action
        {
            get
            {
                return e =>
                {
                    if (e == null)
                    {
                        throw new ArgumentNullException("e");
                    }

                    var ev = e as TEvent;
                    if (ev != null && filters.All(filter => filter(ev)))
                    {
                        var handler = handlerResolver();
                        if (handler != null && handler.IsSatisfiedBy(ev))
                        {
                            handler.Handle(ev);
                        }
                    }
                };
            }
        }
    }
}