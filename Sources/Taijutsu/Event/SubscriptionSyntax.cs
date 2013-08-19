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

using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    public interface ISubscriptionSyntax<out TEvent> : IHiddenObjectMembers where TEvent : IEvent
    {
        ISubscriptionSyntax<TEvent> Where(Func<TEvent, bool> filter);

        IDisposable Subscribe(Action<TEvent> handler, int priority = 0);
    }

    public class SubscriptionSyntax<TEvent> : ISubscriptionSyntax<TEvent> where TEvent : class, IEvent
    {
        private readonly Func<IEventHandlingSettings, IDisposable> subscribeImplementation;

        private readonly List<Func<TEvent, bool>> filters = new List<Func<TEvent, bool>>();

        public SubscriptionSyntax(Func<IEventHandlingSettings, IDisposable> subscribeImplementation, IEnumerable<Func<TEvent, bool>> filters = null)
        {
            this.subscribeImplementation = subscribeImplementation;

            if (filters != null)
            {
                this.filters.AddRange(filters);
            }
        }

        public Func<IEventHandlingSettings, IDisposable> SubscribeImplementation
        {
            get { return subscribeImplementation; }
        }

        public List<Func<TEvent, bool>> Filters
        {
            get { return filters; }
        }

        public ISubscriptionSyntax<TEvent> Where(Func<TEvent, bool> filter)
        {
            filters.Add(filter);
            return new SubscriptionSyntax<TEvent>(subscribeImplementation, filters);
        }

        public IDisposable Subscribe(Action<TEvent> handler, int priority = 0)
        {
            return subscribeImplementation(new TypedHandlingSettings<TEvent>(() => new SpecEventHandler<TEvent>(handler), filters, priority));
        }
    }
}