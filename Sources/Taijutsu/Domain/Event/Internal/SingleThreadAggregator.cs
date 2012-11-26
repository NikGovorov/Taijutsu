#region License

// Copyright 2009-2012 Taijutsu.
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Taijutsu.Domain.Event.Syntax.Subscribing;

namespace Taijutsu.Domain.Event.Internal
{
    public class SingleThreadAggregator : IEventAggregator, IEventStream, IEventStreamFilter, IDisposable
    {
        protected IDictionary<Type, IList<IInternalEventHandler>> handlersDictionary =
            new Dictionary<Type, IList<IInternalEventHandler>>();

        protected IDictionary<Type, IEnumerable<Type>> targets = new Dictionary<Type, IEnumerable<Type>>();

        public virtual IEventStream OnStream
        {
            get { return this; }
        }

        public virtual SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return OnStream.Of<TEvent>();
        }

        public virtual Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            return OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        public virtual  Action Subscribe<TEvent>(IHandler<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            return OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        public virtual void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            var eventHandlers = new List<IInternalEventHandler>();

            foreach (var targetType in PotentialSubscribers(ev.GetType()))
            {
                IList<IInternalEventHandler> localHandlers;
                if (handlersDictionary.TryGetValue(targetType, out localHandlers))
                {
                    eventHandlers.AddRange(localHandlers);
                }
            }

            foreach (var handler in eventHandlers.OrderByDescending(h => h.Priority))
            {
                handler.HandlerAction(ev);
            }
        }

        Syntax.Publishing.DueToSyntax.Init<TFact> IEventAggregator.DueTo<TFact>(TFact fact)
        {
            return new Syntax.Publishing.DueToSyntax.InitImpl<TFact>(Publish, fact, null, null);
        }

        DueToSyntax.Init<TFact> IEventStreamFilter.DueTo<TFact>()
        {
            return new DueToSyntax.InitImpl<TFact>(Subscribe);
        }

        InitiatedBySyntax.Init<TEntity> IEventStreamFilter.InitiatedBy<TEntity>()
        {
            return new InitiatedBySyntax.InitImpl<TEntity>(Subscribe);
        }

        AddressedToSyntax.Init<TEntity> IEventStreamFilter.AddressedTo<TEntity>()
        {
            return new AddressedToSyntax.InitImpl<TEntity>(Subscribe);
        }

        IEventStreamFilter IEventStream.OfEvents
        {
            get { return this; }
        }

        SubscriptionSyntax.All<TEvent> IEventStream.Of<TEvent>()
        {
            return new SubscriptionSyntax.AllImpl<TEvent>(Subscribe);
        }

        protected virtual IEnumerable<Type> PotentialSubscribers(Type type)
        {
            IEnumerable<Type> targetsForType;
            if (!targets.TryGetValue(type, out targetsForType))
            {
                targetsForType =
                    type.GetInterfaces().Where(i => typeof (IEvent).IsAssignableFrom(i)).Union(
                        EventTypeHierarchy(type).Reverse()).ToArray();
                CachePotentialSubscribers(type, targetsForType);
            }
            return targetsForType as Type[] ?? targetsForType.ToArray();
        }

        protected virtual IEnumerable<Type> EventTypeHierarchy(Type type)
        {
            if (typeof (IEvent).IsAssignableFrom(type))
            {
                yield return type;
                foreach (var subtype in EventTypeHierarchy(type.BaseType))
                {
                    yield return subtype;
                }
            }
        }

        protected virtual void CachePotentialSubscribers(Type type, IEnumerable<Type> potentialSubscribers)
        {
            targets[type] = potentialSubscribers;
        }

        protected virtual Action Subscribe(IInternalEventHandler handler)
        {
            IList<IInternalEventHandler> handlers;
            if (!handlersDictionary.TryGetValue(handler.EventType, out handlers))
            {
                handlersDictionary.Add(handler.EventType, new List<IInternalEventHandler> {handler});
            }
            else
            {
                handlers.Add(handler);
            }

            return GenerateUnsubscriptionAction(handler);
        }

        protected virtual Action GenerateUnsubscriptionAction(IInternalEventHandler handler)
        {
            return delegate
                {
                    IList<IInternalEventHandler> handlers;
                    if (handlersDictionary.TryGetValue(handler.EventType, out handlers))
                    {
                        handlers.Remove(handler);
                    }
                };
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        protected virtual void Dispose()
        {
            handlersDictionary.Clear();
        }

    }
}