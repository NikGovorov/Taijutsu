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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Taijutsu.Event.Internal
{
    public class SingleThreadAggregator : IEvents, IResettable
    {
        private static readonly object sync = new object();

        private static IDictionary<Type, IEnumerable<Type>> targets = new Dictionary<Type, IEnumerable<Type>>();

        private IDictionary<Type, IList<IEventHandlingSettings>> handlers = new Dictionary<Type, IList<IEventHandlingSettings>>();

        protected virtual IDictionary<Type, IList<IEventHandlingSettings>> Handlers
        {
            get { return handlers; }
            set { handlers = value; }
        }

        protected virtual IDictionary<Type, IEnumerable<Type>> Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        public IEvents<TEvent> OfType<TEvent>() where TEvent : class, IEvent
        {
            return new Events<TEvent>(this);
        }

        public ISubscriptionSyntax<TEvent> Where<TEvent>(Func<TEvent, bool> filter) where TEvent : class, IEvent
        {
            return new SubscriptionSyntax<TEvent>(Subscribe, new List<Func<TEvent, bool>> { filter });
        }

        public virtual IDisposable Subscribe(IEventHandlingSettings handlingSettings)
        {
            if (!typeof(IEvent).IsAssignableFrom(handlingSettings.Type))
            {
                throw new Exception(string.Format("'{0}' does not implement '{1}'.", handlingSettings.Type, typeof(IEvent)));
            }

            IList<IEventHandlingSettings> internalEventHandlers;

            if (!Handlers.TryGetValue(handlingSettings.Type, out internalEventHandlers))
            {
                Handlers.Add(handlingSettings.Type, new List<IEventHandlingSettings> { handlingSettings });
            }
            else
            {
                internalEventHandlers.Add(handlingSettings);
            }

            return UnsubscriptionAction(handlingSettings);
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler, int priority = 0) where TEvent : class, IEvent
        {
            return new SubscriptionSyntax<TEvent>(Subscribe).Subscribe(handler, priority);
        }

        public virtual void Publish(object ev)
        {
            if (ev == null)
            {
                throw new ArgumentException("ev");
            }

            if (ev is IEvent)
            {
                var eventHandlers = new List<IEventHandlingSettings>();

                var potentialSubscriberTypes = PotentialSubscriberTypes(ev.GetType());

                foreach (var targetType in potentialSubscriberTypes)
                {
                    IList<IEventHandlingSettings> internalEventHandlers;
                    if (Handlers.TryGetValue(targetType, out internalEventHandlers))
                    {
                        eventHandlers.AddRange(internalEventHandlers);
                    }
                }

                foreach (var handler in eventHandlers.OrderByDescending(h => h.Priority))
                {
                    handler.Action(ev);
                }
            }
        }

        public void Publish<TEvent>(TEvent ev) where TEvent : class, IEvent
        {
            Publish(ev as object);
        }

        void IResettable.Reset()
        {
            Reset();
        }

        protected virtual IEnumerable<Type> PotentialSubscriberTypes(Type type)
        {
            IEnumerable<Type> targetsForType;

            if (!Targets.TryGetValue(type, out targetsForType))
            {
                targetsForType = type.GetInterfaces().Where(i => typeof(IEvent).IsAssignableFrom(i)).Union(EventTypeHierarchy(type).Reverse()).ToArray();

                CachePotentialSubscriberTypes(type, targetsForType);
            }

            // ReSharper disable PossibleMultipleEnumeration
            return targetsForType as Type[] ?? targetsForType.ToArray();

            // ReSharper restore PossibleMultipleEnumeration
        }

        protected virtual IEnumerable<Type> EventTypeHierarchy(Type type)
        {
            if (!typeof(IEvent).IsAssignableFrom(type))
            {
                yield break;
            }

            yield return type;

            foreach (var subtype in EventTypeHierarchy(type.BaseType))
            {
                yield return subtype;
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
                        if (Targets.ContainsKey(type))
                        {
                            return;
                        }

                        var newTargets = new Dictionary<Type, IEnumerable<Type>>(Targets);
                        newTargets[type] = potentialSubscriberTypes;
                        Targets = newTargets;
                    }
                })
                .ContinueWith(
                    t => Trace.TraceError(t.Exception == null ? string.Format("Taijutsu: Error occurred during caching event subscribers for '{0}'.", type) : t.Exception.ToString()), 
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            // ReSharper restore ImplicitlyCapturedClosure
        }

        protected virtual IDisposable UnsubscriptionAction(IEventHandlingSettings handlingSettings)
        {
            Action action = delegate
            {
                IList<IEventHandlingSettings> internalEventHandlers;
                if (Handlers.TryGetValue(handlingSettings.Type, out internalEventHandlers))
                {
                    internalEventHandlers.Remove(handlingSettings);
                }
            };

            return action.AsDisposable();
        }

        protected virtual void Reset()
        {
            Handlers.Clear();
        }

        private class Events<TEvent> : IEvents<TEvent> where TEvent : class, IEvent
        {
            private readonly IEvents implementation;

            public Events(IEvents implementation)
            {
                this.implementation = implementation;
            }

            public ISubscriptionSyntax<TEvent> Where(Func<TEvent, bool> filter)
            {
                return implementation.Where(filter);
            }

            public IDisposable Subscribe(Action<TEvent> handler, int priority = 0)
            {
                return implementation.Subscribe(handler, priority);
            }

            public void Publish(TEvent ev)
            {
                implementation.Publish(ev);
            }
        }
    }
}