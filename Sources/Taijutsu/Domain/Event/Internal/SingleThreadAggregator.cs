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
namespace Taijutsu.Domain.Event.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class SingleThreadAggregator : IEventAggregator, IEventStream, IDisposable
    {
        private static readonly object sync = new object();

        private static IDictionary<Type, IEnumerable<Type>> targets = new Dictionary<Type, IEnumerable<Type>>();

        private IDictionary<Type, IList<IInternalEventHandler>> handlers = new Dictionary<Type, IList<IInternalEventHandler>>();

        public virtual IEventStream OnStream
        {
            get
            {
                return this;
            }
        }

        protected virtual IDictionary<Type, IList<IInternalEventHandler>> Handlers
        {
            get
            {
                return this.handlers;
            }

            set
            {
                this.handlers = value;
            }
        }

        protected virtual IDictionary<Type, IEnumerable<Type>> Targets
        {
            get
            {
                return targets;
            }

            set
            {
                targets = value;
            }
        }

        public virtual SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return this.OnStream.Of<TEvent>();
        }

        public virtual Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            return this.OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        public virtual Action Subscribe<TEvent>(IHandler<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            return this.OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        public virtual void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            var eventHandlers = new List<IInternalEventHandler>();

            var potentialSubscriberTypes = this.PotentialSubscriberTypes(ev.GetType());

            foreach (var targetType in potentialSubscriberTypes)
            {
                IList<IInternalEventHandler> internalEventHandlers;
                if (this.Handlers.TryGetValue(targetType, out internalEventHandlers))
                {
                    eventHandlers.AddRange(internalEventHandlers);
                }
            }

            foreach (var handler in eventHandlers.OrderByDescending(h => h.Priority))
            {
                handler.HandlerAction(ev);
            }
        }

        SubscriptionSyntax.All<TEvent> IEventStream.Of<TEvent>()
        {
            return new SubscriptionSyntax.AllImpl<TEvent>(this.Subscribe);
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        protected virtual IEnumerable<Type> PotentialSubscriberTypes(Type type)
        {
            IEnumerable<Type> targetsForType;

            if (!this.Targets.TryGetValue(type, out targetsForType))
            {
                targetsForType = type.GetInterfaces().Where(i => typeof(IEvent).IsAssignableFrom(i)).Union(this.EventTypeHierarchy(type).Reverse()).ToArray();

                this.CachePotentialSubscriberTypes(type, targetsForType);
            }

            // ReSharper disable PossibleMultipleEnumeration
            return targetsForType as Type[] ?? targetsForType.ToArray();

            // ReSharper restore PossibleMultipleEnumeration
        }

        protected virtual IEnumerable<Type> EventTypeHierarchy(Type type)
        {
            if (typeof(IEvent).IsAssignableFrom(type))
            {
                yield return type;
                foreach (var subtype in this.EventTypeHierarchy(type.BaseType))
                {
                    yield return subtype;
                }
            }
        }

        protected virtual void CachePotentialSubscriberTypes(Type type, IEnumerable<Type> potentialSubscriberTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (potentialSubscriberTypes == null)
            {
                throw new ArgumentNullException("potentialSubscriberTypes");
            }

            // ReSharper disable ImplicitlyCapturedClosure
            Task.Factory.StartNew(
                () =>
                    {
                        lock (sync)
                        {
                            if (!this.Targets.ContainsKey(type))
                            {
                                var newTargets = new Dictionary<Type, IEnumerable<Type>>(this.Targets);
                                newTargets[type] = potentialSubscriberTypes;
                                this.Targets = newTargets;
                            }
                        }
                    })
                .ContinueWith(
                    t => Trace.TraceError(t.Exception == null ? string.Format("Error occurred during caching event subscribers for '{0}'.", type) : t.Exception.ToString()), 
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            // ReSharper restore ImplicitlyCapturedClosure
        }

        protected virtual Action Subscribe(IInternalEventHandler handler)
        {
            IList<IInternalEventHandler> internalEventHandlers;

            if (!this.Handlers.TryGetValue(handler.EventType, out internalEventHandlers))
            {
                this.Handlers.Add(handler.EventType, new List<IInternalEventHandler> { handler });
            }
            else
            {
                internalEventHandlers.Add(handler);
            }

            return this.UnsubscriptionAction(handler);
        }

        protected virtual Action UnsubscriptionAction(IInternalEventHandler handler)
        {
            return delegate
                {
                    IList<IInternalEventHandler> internalEventHandlers;
                    if (this.Handlers.TryGetValue(handler.EventType, out internalEventHandlers))
                    {
                        internalEventHandlers.Remove(handler);
                    }
                };
        }

        protected virtual void Dispose()
        {
            this.Handlers.Clear();
        }
    }
}