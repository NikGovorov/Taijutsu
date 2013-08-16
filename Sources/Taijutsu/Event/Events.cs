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

        public static IEventStream OnStream
        {
            get { return globalEvents.OnStream; }
        }

        IEventStream IEvents.OnStream
        {
            get { return OnStream; }
        }

        public static SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return globalEvents.OnStreamOf<TEvent>();
        }

        public static Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            return globalEvents.Subscribe(subscriber, priority);
        }

        public static Action Subscribe<TEvent>(IEventHandler<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            return globalEvents.Subscribe(subscriber, priority);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public static void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            if (Equals(ev, default(TEvent)))
            {
                throw new ArgumentNullException("ev");
            }

            var localEventAggregator = FindLocalEventAggregator();

            if (localEventAggregator != null)
            {
                localEventAggregator.Publish(ev);
            }

            globalEvents.Publish(ev);
        }

        SubscriptionSyntax.All<TEvent> IEvents.OnStreamOf<TEvent>()
        {
            return Current.OnStreamOf<TEvent>();
        }

        Action IEvents.Subscribe<TEvent>(Action<TEvent> subscriber, int priority)
        {
            return Current.Subscribe(subscriber, priority);
        }

        Action IEvents.Subscribe<TEvent>(IEventHandler<TEvent> subscriber, int priority)
        {
            return Current.Subscribe(subscriber, priority);
        }

        void IEvents.Publish<TEvent>(TEvent ev)
        {
            Current.Publish(ev);
        }

        private static IEvents FindLocalEventAggregator()
        {
            return LogicContext.FindData(LocalEventAggregatorName) as IEvents;
        }
    }
}