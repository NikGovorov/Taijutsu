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

using Taijutsu.Annotation;
using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    public sealed class Events : IEvents
    {
        private const string LocalEventAggregatorName = "Taijutsu.LocalEventAggregator";

        private static readonly IEvents globalEvents = new MultiThreadAggregator();

        private Events()
        {
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
                var localEvents = FindLocalEventAggregator();

                if (localEvents == null)
                {
                    localEvents = new SingleThreadAggregator();
                    LogicContext.SetData(LocalEventAggregatorName, localEvents);
                }

                return localEvents;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDisposable Subscribe([NotNull] IEventHandlingSettings handlingSettings)
        {
            if (handlingSettings == null)
            {
                throw new ArgumentNullException("handlingSettings");
            }

            return globalEvents.Subscribe(handlingSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Publish([NotNull] object ev)
        {
            var localEventAggregator = FindLocalEventAggregator();

            if (localEventAggregator != null)
            {
                localEventAggregator.Publish(ev);
            }

            globalEvents.Publish(ev);
        }

        public static IEvents OfAnyType()
        {
            return new Events();
        }

        public static IEvents<TEvent> OfType<TEvent>() where TEvent : class, IEvent
        {
            return new Events<TEvent>();
        }

        public static ISubscriptionSyntax<TEvent> Where<TEvent>([NotNull] Func<TEvent, bool> filter) where TEvent : class, IEvent
        {
            return globalEvents.Where(filter);
        }

        public static IDisposable Subscribe<TEvent>([NotNull] Action<TEvent> handler, int priority = 0) where TEvent : class, IEvent
        {
            return globalEvents.Subscribe(handler, priority);
        }

        public static void Publish<TEvent>([NotNull] TEvent ev) where TEvent : class, IEvent
        {
            Publish(ev as object);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDisposable IEvents.Subscribe(IEventHandlingSettings handlingSettings)
        {
            return Subscribe(handlingSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IEvents.Publish(object ev)
        {
            Publish(ev);
        }

        IEvents<TEvent> IEvents.OfType<TEvent>()
        {
            return OfType<TEvent>();
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

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Acceptable for generic and non-generic classes.")]
    public sealed class Events<TEvent> : IEvents<TEvent> where TEvent : class, IEvent
    {
        internal Events()
        {
        }

        public static ISubscriptionSyntax<TEvent> Where([NotNull] Func<TEvent, bool> filter)
        {
            return Events.Where(filter);
        }

        public static IDisposable Subscribe([NotNull] Action<TEvent> handler, int priority = 0)
        {
            return Events.Subscribe(handler, priority);
        }

        public static void Publish([NotNull] TEvent ev)
        {
            Events.Publish(ev);
        }

        IDisposable ISubscriptionSyntax<TEvent>.Subscribe(Action<TEvent> handler, int priority)
        {
            return Subscribe(handler, priority);
        }

        ISubscriptionSyntax<TEvent> ISubscriptionSyntax<TEvent>.Where(Func<TEvent, bool> filter)
        {
            return Where(filter);
        }

        void IEvents<TEvent>.Publish(TEvent ev)
        {
            Publish(ev);
        }
    }
}