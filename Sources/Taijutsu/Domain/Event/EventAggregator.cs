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
namespace Taijutsu.Domain.Event
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    using Taijutsu.Domain.Event.Internal;

    public static class EventAggregator
    {
        private const string LocalEventAggregatorName = "Taijutsu.LocalEventAggregator";

        private static readonly IEventAggregator globalEventAggregator = new MultiThreadAggregator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEventAggregator Global
        {
            get
            {
                return globalEventAggregator;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEventAggregator Local
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
            get
            {
                return globalEventAggregator.OnStream;
            }
        }

        public static SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return globalEventAggregator.OnStreamOf<TEvent>();
        }

        public static Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            return globalEventAggregator.Subscribe(subscriber, priority);
        }

        public static Action Subscribe<TEvent>(IHandler<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            return globalEventAggregator.Subscribe(subscriber, priority);
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

            globalEventAggregator.Publish(ev);
        }

        private static IEventAggregator FindLocalEventAggregator()
        {
            return LogicContext.FindData(LocalEventAggregatorName) as IEventAggregator;
        }
    }
}