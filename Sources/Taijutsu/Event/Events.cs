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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    [PublicApi]
    public sealed class Events : IEvents
    {
        private const string LocalEventAggregatorName = "Taijutsu.LocalEventAggregator";

        private static readonly object sync = new object();

        private static readonly IEvents globalEvents = new MultiThreadAggregator();

        private static IEvents current;

        private Events()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEvents Current
        {
            get
            {
                if (current == null)
                {
                    lock (sync)
                    {
                        if (current == null)
                        {
                            current = new Events();
                        }
                    }
                }

                return current;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEvents Global
        {
            get { return globalEvents; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEvents Local
        {
            get
            {
                var localEventAggregator = FindLocalEventAggregator();

                if (localEventAggregator == null)
                {
                    localEventAggregator = new SingleThreadAggregator();
                    LogicContext.SetData(LocalEventAggregatorName, localEventAggregator);
                }

                return localEventAggregator;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDisposable Subscribe(IEventHandlerSettings handlerSettings)
        {
            if (handlerSettings == null)
            {
                throw new ArgumentNullException("handlerSettings");
            }

            return globalEvents.Subscribe(handlerSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Publish(object ev)
        {
            var localEventAggregator = FindLocalEventAggregator();

            if (localEventAggregator != null)
            {
                localEventAggregator.Publish(ev);
            }

            globalEvents.Publish(ev);
        }

        public static ISubscriptionSyntax<TEvent> Where<TEvent>(Func<TEvent, bool> filter) where TEvent : class, IEvent
        {
            return globalEvents.Where(filter);
        }

        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler, int priority = 0) where TEvent : class, IEvent
        {
            return globalEvents.Subscribe(handler, priority);
        }

        public static IDisposable Subscribe<TEvent>(Action<IEventBatch<TEvent>> handler, int priority = 0) where TEvent : class, IEvent
        {
            return null;
        }

        public static void Publish<TEvent>(TEvent ev) where TEvent : class, IEvent
        {
            Publish(ev as object);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDisposable IEvents.Subscribe(IEventHandlerSettings handlerSettings)
        {
            return Subscribe(handlerSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IEvents.Publish(object ev)
        {
            Publish(ev);
        }

        ISubscriptionSyntax<TEvent> IEvents.Where<TEvent>(Func<TEvent, bool> filter)
        {
            return Where(filter);
        }

        IDisposable IEvents.Subscribe<TEvent>(Action<TEvent> handler, int priority)
        {
            return Subscribe(handler, priority);
        }

        void IEvents.Publish<TEvent>(TEvent ev)
        {
            Publish(ev);
        }

        private static IEvents FindLocalEventAggregator()
        {
            return LogicContext.FindData(LocalEventAggregatorName) as IEvents;
        }
    }

    [PublicApi]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Acceptable for generic and non-generic classes.")]
    public sealed class Events<TEvent> where TEvent : class, IEvent
    {
        public static ISubscriptionSyntax<TEvent> Where(Func<TEvent, bool> filter)
        {
            return Events.Where(filter);
        }

        public static IDisposable Subscribe(Action<TEvent> handler, int priority = 0)
        {
            return Events.Subscribe(handler, priority);
        }

        public static void Publish(TEvent ev)
        {
            Events.Publish(ev);
        }
    }
}