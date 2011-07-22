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
    public class LocalEventAggregator : IScopeSupervisor<ISubscriptionScope>, IEventAggregator
    {
        private readonly Action destroyAction;
        private readonly IList<SubscriptionScope> scopes = new List<SubscriptionScope>();

        public LocalEventAggregator(Action destroyAction)
        {
            this.destroyAction = destroyAction;
        }

        #region IEventAggregator Members

        public virtual void Raise<TEvent>(TEvent ev) where TEvent : class
        {
            Type evType = ev.GetType();

            foreach (var scope in scopes)
            {
                scope.ScopeHandlers.Where(handler => handler.EventType == evType && handler.Suitable(ev))
                    .Select(handler =>
                                {
                                    handler.HandlerAction(ev);
                                    return new object();
                                }).ToArray();
            }
        }

        #endregion

        #region IScopeSupervisor<ISubscriptionScope> Members

        public virtual ISubscriptionScope DefineScope()
        {
            var scope = new SubscriptionScope(this);
            scopes.Add(scope);
            return scope;
        }

        #endregion

        #region Nested type: SubscriptionScope

        protected class SubscriptionScope : ISubscriptionScope
        {
            private LocalEventAggregator aggregator;
            private IList<IEventHandler> scopeHandlers = new List<IEventHandler>();

            public SubscriptionScope(LocalEventAggregator aggregator)
            {
                this.aggregator = aggregator;
            }

            protected internal virtual IList<IEventHandler> ScopeHandlers
            {
                get { return scopeHandlers; }
            }

            #region ISubscriptionScope Members

            public virtual IConditionStep<TEvent> ReactOn<TEvent>() where TEvent : class
            {
                return new SubscriptionBuilder<TEvent>(Subscribe);
            }

            void IDisposable.Dispose()
            {
                scopeHandlers = null;

                if (aggregator.scopes.Count > 0)
                {
                    if (aggregator.scopes[0] == this)
                    {
                        aggregator.scopes.Clear();
                    }
                    else
                    {
                        aggregator.scopes.Remove(this);
                    }
                }


                if (aggregator.scopes.Count == 0)
                {
                    aggregator.destroyAction();
                }

                aggregator = null;
            }

            #endregion

            protected virtual Action Subscribe<TEvent>(Action<TEvent> handlingAction, Predicate<TEvent> predicate)
                where TEvent : class
            {
                var handler = new EventHandler<TEvent>(handlingAction, predicate);

                if (scopeHandlers != null)
                {
                    scopeHandlers.Add(handler);
                    return GenerateUnsubscriptionAction(handler);
                }
                return delegate { };
            }

            protected virtual Action GenerateUnsubscriptionAction(IEventHandler handler)
            {
                return delegate
                           {
                               if (scopeHandlers != null)
                               {
                                   scopeHandlers.Remove(handler);
                               }
                           };
            }
        }

        #endregion
    }
}