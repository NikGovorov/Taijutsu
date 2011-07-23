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
using Taijutsu.Domain;
using Taijutsu.Infrastructure.Internal;
using Taijutsu.Domain.Event;

namespace Taijutsu.Infrastructure
{
    public enum After
    {
        Successed,
        Failed
    }

    public enum Before
    {
        Successed,
        Failed
    }

    public static class EventUnitOfWorkEx
    {
        private const string SubscriptionScopeKeyName = "UnitSubscriptionScope";


        public static ISubscriptionStep<TEvent> ReactOn<TEvent>(this UnitOfWork uow) where TEvent : class
        {
            return new SubscriptionBuilder<TEvent>(RegisterScopeInUnitOfWork(uow), uow);
        }

        public static ISubscriptionStep<TEvent> ReactOn<TEvent>(this UnitOfWork uow, After after)
            where TEvent : class
        {
            return new SubscriptionBuilder<TEvent>(RegisterScopeInUnitOfWork(uow), uow, after: after);
        }

        public static ISubscriptionStep<TEvent> ReactOn<TEvent>(this UnitOfWork uow, Before before)
            where TEvent : class
        {
            return new SubscriptionBuilder<TEvent>(RegisterScopeInUnitOfWork(uow), uow, before: before);
        }


        internal static ISubscriptionScope RegisterScopeInUnitOfWork(IAdvancedUnitOfWork advancedUnitOfWork)
        {
            IDisposable disposable;

            ISubscriptionScope scope;

            if (!advancedUnitOfWork.Extension.TryGetValue(SubscriptionScopeKeyName, out disposable))
            {
                scope = EventAggregator.Local.DefineScope();
                advancedUnitOfWork.Extension[SubscriptionScopeKeyName] = scope;
            }
            else
            {
                scope = (ISubscriptionScope) disposable;
            }
            return scope;
        }

        #region Nested type: IConditionStep

        public interface IConditionStep<out TEvent> : ISubscriptionStep<TEvent> where TEvent : class
        {
            ISubscriptionStep<TEvent> Conditionally(Predicate<TEvent> condition);
        }

        #endregion

        #region Nested type: ISubscriptionStep

        public interface ISubscriptionStep<out TEvent> : IHideObjectMembers where TEvent : class
        {
            UnitOfWork By(Action<TEvent> handler);
            UnitOfWork By(ISubscriber<TEvent> subscriber);
        }

        #endregion

        #region Nested type: SubscriptionBuilder

        private class SubscriptionBuilder<TEvent> : IConditionStep<TEvent> where TEvent : class
        {
            private readonly After? after;
            private readonly Before? before;
            private readonly Predicate<TEvent> predicate = e => true;
            private readonly ISubscriptionScope scope;
            private readonly UnitOfWork unitOfWork;

            public SubscriptionBuilder(ISubscriptionScope scope, UnitOfWork unitOfWork,
                                       Predicate<TEvent> predicate = null, Before? before = null, After? after = null)
            {
                this.unitOfWork = unitOfWork;
                this.scope = scope;

                if (predicate != null)
                {
                    this.predicate = predicate;
                }

                if (before != null)
                {
                    this.before = before;
                }

                if (after != null)
                {
                    this.after = after;
                }
            }

            #region IConditionStep<TEvent> Members

            public virtual UnitOfWork By(Action<TEvent> handler)
            {
                if (before == null && after == null)
                {
                    scope.ReactOn<TEvent>().Conditionally(predicate).By(handler);
                }

                if (after == null)
                {
                    switch (before)
                    {
                        case Before.Successed:
                            scope.ReactOn<TEvent>().Conditionally(predicate).By(
                                ev => { unitOfWork.Advanced.BeforeSuccessed += (() => handler(ev)); });
                            break;
                        case Before.Failed:
                            scope.ReactOn<TEvent>().Conditionally(predicate).By(
                                ev => { unitOfWork.Advanced.BeforeFailed += (() => handler(ev)); });
                            break;
                    }
                }

                if (before == null)
                {
                    switch (after)
                    {
                        case After.Successed:
                            scope.ReactOn<TEvent>().Conditionally(predicate).By(
                                ev => { unitOfWork.Advanced.AfterSuccessed += (() => handler(ev)); });
                            break;
                        case After.Failed:
                            scope.ReactOn<TEvent>().Conditionally(predicate).By(
                                ev => { unitOfWork.Advanced.AfterFailed += (() => handler(ev)); });
                            break;
                    }
                }

                return unitOfWork;
            }

            public virtual UnitOfWork By(ISubscriber<TEvent> subscriber)
            {
                return By(subscriber.Handle);
            }

            public virtual ISubscriptionStep<TEvent> Conditionally(Predicate<TEvent> condition)
            {
                return new SubscriptionBuilder<TEvent>(scope, unitOfWork, condition, before, after);
            }

            #endregion
        }

        #endregion
    }
}