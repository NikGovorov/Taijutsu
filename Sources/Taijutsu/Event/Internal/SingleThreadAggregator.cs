﻿// Copyright 2009-2013 Nikita Govorov
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

        private IDictionary<Type, IList<IEventHandlerSettings>> handlers = new Dictionary<Type, IList<IEventHandlerSettings>>();

        protected virtual IDictionary<Type, IList<IEventHandlerSettings>> Handlers
        {
            get { return handlers; }
            set { handlers = value; }
        }

        protected virtual IDictionary<Type, IEnumerable<Type>> Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        public ISubscriptionSyntax<TEvent> Where<TEvent>(Func<TEvent, bool> filter) where TEvent : class, IEvent
        {
            return new SubscriptionSyntax<TEvent>(Subscribe, new List<Func<TEvent, bool>> { filter });
        }

        public virtual IDisposable Subscribe(IEventHandlerSettings handlerSettings)
        {
            if (!typeof(IEvent).IsAssignableFrom(handlerSettings.Type))
            {
                throw new Exception(string.Format("'{0}' does not implement '{1}'.", handlerSettings.Type, typeof(IEvent)));
            }

            IList<IEventHandlerSettings> internalEventHandlers;

            if (!Handlers.TryGetValue(handlerSettings.Type, out internalEventHandlers))
            {
                Handlers.Add(handlerSettings.Type, new List<IEventHandlerSettings> { handlerSettings });
            }
            else
            {
                internalEventHandlers.Add(handlerSettings);
            }

            return UnsubscriptionAction(handlerSettings);
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
                var eventHandlers = new List<IEventHandlerSettings>();

                var potentialSubscriberTypes = PotentialSubscriberTypes(ev.GetType());

                foreach (var targetType in potentialSubscriberTypes)
                {
                    IList<IEventHandlerSettings> internalEventHandlers;
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
                    t => Trace.TraceError(t.Exception == null ? string.Format("Error occurred during caching event subscribers for '{0}'.", type) : t.Exception.ToString()), 
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            // ReSharper restore ImplicitlyCapturedClosure
        }

        protected virtual IDisposable UnsubscriptionAction(IEventHandlerSettings handlerSettings)
        {
            Action action = delegate
            {
                IList<IEventHandlerSettings> internalEventHandlers;
                if (Handlers.TryGetValue(handlerSettings.Type, out internalEventHandlers))
                {
                    internalEventHandlers.Remove(handlerSettings);
                }
            };

            return action.AsDisposable();
        }

        protected virtual void Reset()
        {
            Handlers.Clear();
        }
    }
}