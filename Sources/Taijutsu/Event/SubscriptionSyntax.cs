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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    public static class SubscriptionSyntax
    {
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1302:InterfaceNamesMustBeginWithI", 
            Justification = "Reviewed. Interfaces which participate as part of fluent syntax can violate the rule.")]
        public interface All<out TSource> : IHiddenObjectMembers
        {
            All<TSource> Where(Func<TSource, bool> filter);

            Action Subscribe(Action<TSource> subscriber, int priority = 0);

            Action Subscribe(IEventHandler<TSource> subscriber, int priority = 0);
        }

        internal class AllImpl<TEvent> : All<TEvent> where TEvent : class
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;

            private readonly List<Func<TEvent, bool>> eventFilters = new List<Func<TEvent, bool>>();

            internal AllImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Func<TEvent, bool>> eventFilters = null)
            {
                this.addHadlerAction = addHadlerAction;

                if (eventFilters != null)
                {
                    this.eventFilters.AddRange(eventFilters);
                }
            }

            public Func<IInternalEventHandler, Action> AddHadlerAction
            {
                get { return addHadlerAction; }
            }

            public IEnumerable<Func<TEvent, bool>> EventFilters
            {
                get { return eventFilters; }
            }

            public All<TEvent> Where(Func<TEvent, bool> filter)
            {
                eventFilters.Add(filter);
                return new AllImpl<TEvent>(AddHadlerAction, eventFilters);
            }

            public Action Subscribe(Action<TEvent> subscriber, int priority = 0)
            {
                return AddHadlerAction(new InternalEventHandler<TEvent>(subscriber, e => eventFilters.All(f => f(e)), priority));
            }

            public Action Subscribe(IEventHandler<TEvent> subscriber, int priority = 0)
            {
                return Subscribe(subscriber.Handle, priority);
            }
        }

        // ReSharper restore InconsistentNaming
    }
}