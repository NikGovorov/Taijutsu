// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.


using System;
using System.Collections.Generic;
using System.Linq;

namespace Taijutsu.Domain.Event
{
    public class SystemEventAggregator : ISubscriptionFeature, IScopeSupervisor<ISubscriptionScope>, IEventAggregator,
                                         IDisposable
    {
        private readonly object sync = new object();
        private IDictionary<Type, IList<IEventHandler>> handlers = new Dictionary<Type, IList<IEventHandler>>();

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            lock (sync)
            {
                handlers = new Dictionary<Type, IList<IEventHandler>>();
            }
        }

        #endregion

        #region IEventAggregator Members

        public virtual void Raise<TEvent>(TEvent ev) where TEvent : class
        {
            Type evType = ev.GetType();
            IList<IEventHandler> specialHandlers;
            if (handlers.TryGetValue(evType, out specialHandlers))
            {
                IEnumerable<IEventHandler> suitableHandlers = specialHandlers.Where(handler => handler.Suitable(ev));
                foreach (var suitableHandler in suitableHandlers)
                {
                    suitableHandler.HandlerAction(ev);
                }
            }
        }

        #endregion

        #region IScopeSupervisor<ISubscriptionScope> Members

        public virtual ISubscriptionScope DefineScope()
        {
            return new SubscriptionScope(this);
        }

        #endregion

        #region ISubscriptionFeature Members

        public virtual IConditionStep<TEvent> ReactOn<TEvent>() where TEvent : class
        {
            return new SubscriptionBuilder<TEvent>(Subscribe);
        }

        #endregion

        protected virtual Action Subscribe<TEvent>(Action<TEvent> handlingAction, Predicate<TEvent> predicate)
            where TEvent : class
        {
            var handler = new EventHandler<TEvent>(handlingAction, predicate);
            lock (sync)
            {
                IList<IEventHandler> specialHandlers;
                if (!handlers.TryGetValue(typeof (TEvent), out specialHandlers))
                {
                    var newHandlers = new Dictionary<Type, IList<IEventHandler>>(handlers)
                                          {{typeof (TEvent), new List<IEventHandler> {handler}}};
                    handlers = newHandlers;
                }
                else
                {
                    var newSpecialHandlers = new List<IEventHandler>(specialHandlers) {handler};
                    handlers[typeof (TEvent)] = newSpecialHandlers;
                }
            }
            return GenerateUnsubscriptionAction(handler);
        }

        protected virtual Action GenerateUnsubscriptionAction(IEventHandler handler)
        {
            return delegate
                       {
                           lock (sync)
                           {
                               IList<IEventHandler> specialHandlers;
                               if (handlers.TryGetValue(handler.EventType, out specialHandlers))
                               {
                                   var newSpecialHandlers = new List<IEventHandler>(specialHandlers);
                                   newSpecialHandlers.Remove(handler);
                                   handlers[handler.EventType] = newSpecialHandlers;
                               }
                           }
                       };
        }

        #region Nested type: SubscriptionScope

        protected class SubscriptionScope : ISubscriptionScope
        {
            private readonly SystemEventAggregator aggregator;

            private IList<IEventHandler> scopeHandlers = new List<IEventHandler>();

            public SubscriptionScope(SystemEventAggregator aggregator)
            {
                this.aggregator = aggregator;
            }

            #region ISubscriptionScope Members

            void IDisposable.Dispose()
            {
                lock (aggregator.sync)
                {
                    foreach (var scopeHandler in scopeHandlers)
                    {
                        IList<IEventHandler> specialHandlers;
                        if (aggregator.handlers.TryGetValue(scopeHandler.EventType, out specialHandlers))
                        {
                            var newSpecialHandlers = new List<IEventHandler>(specialHandlers);
                            newSpecialHandlers.Remove(scopeHandler);
                            aggregator.handlers[scopeHandler.EventType] = newSpecialHandlers;
                        }
                    }
                    scopeHandlers.Clear();
                }
            }

            public virtual IConditionStep<TEvent> ReactOn<TEvent>() where TEvent : class
            {
                return new SubscriptionBuilder<TEvent>(Subscribe);
            }

            #endregion

            protected virtual Action Subscribe<TEvent>(Action<TEvent> handlingAction, Predicate<TEvent> predicate)
                where TEvent : class
            {
                var handler = new EventHandler<TEvent>(handlingAction, predicate);

                lock (aggregator.sync)
                {
                    IList<IEventHandler> specialHandlers;
                    if (!aggregator.handlers.TryGetValue(typeof (TEvent), out specialHandlers))
                    {
                        var newHandlers = new Dictionary<Type, IList<IEventHandler>>(aggregator.handlers)
                                              {{typeof (TEvent), new List<IEventHandler> {handler}}};
                        aggregator.handlers = newHandlers;
                    }
                    else
                    {
                        var newSpecialHandlers = new List<IEventHandler>(specialHandlers) {handler};
                        aggregator.handlers[typeof (TEvent)] = newSpecialHandlers;
                    }
                    scopeHandlers.Add(handler);
                }

                return delegate
                           {
                               lock (aggregator.sync)
                               {
                                   IList<IEventHandler> specialHandlers;
                                   if (aggregator.handlers.TryGetValue(typeof (TEvent), out specialHandlers))
                                   {
                                       var newSpecialHandlers = new List<IEventHandler>(specialHandlers);
                                       newSpecialHandlers.Remove(handler);
                                       aggregator.handlers[typeof (TEvent)] = newSpecialHandlers;
                                   }
                                   scopeHandlers.Remove(handler);
                               }
                           };
            }
        }

        #endregion
    }
}