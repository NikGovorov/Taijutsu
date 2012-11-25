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
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax.Subscribing
{
    public static class InitiatedBySyntax
    {
        // ReSharper disable InconsistentNaming

        public interface All : IHiddenObjectMembers
        {
            Or Or { get; }
            SubscriptionSyntax.All<IFactEvent<TFact>> DueTo<TFact>() where TFact : IFact;
        }

        public interface All<out TEntity> : IHiddenObjectMembers where TEntity : IEntity
        {
            Or Or { get; }
            Full<TEntity, TFact> DueTo<TFact>() where TFact : IFact;
        }

        internal class AllImpl : All
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Type> initiatorTypes = new List<Type>();

            public AllImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> initiatorTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.initiatorTypes.AddRange(initiatorTypes);
            }

            
            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(initiatorTypes)); }
            }

            SubscriptionSyntax.All<IFactEvent<TFact>> All.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IFactEvent<TFact>>(addHadlerAction, initiatorTypes.Select<Type,Func<IFactEvent<TFact>, bool>>(t => e => t.IsInstanceOfType(e)));
            }
        }

        public interface Full<out TEntity, out TFact> : SubscriptionSyntax.All<IDomainEvent<TEntity, TFact>> 
            where TEntity : IDomainObject where TFact : IFact
        {
            Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0);
            Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0);
        }

        internal class FullImpl<TEntity, TFact> : SubscriptionSyntax.AllImpl<IDomainEvent<TEntity, TFact>>, Full<TEntity, TFact> 
            where TEntity : IDomainObject where TFact : IFact
        {
            internal FullImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Func<IDomainEvent<TEntity, TFact>, bool>> eventFilters = null)
                : base(addHadlerAction, eventFilters)
            {
            }

            public Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0)
            {
                Action<IDomainEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.Initiator)(e.Fact);
                return AddHadlerAction(new InternalEventHandler<IDomainEvent<TEntity, TFact>>(modSubscriber, e => EventFilters.All(f => f(e)), priority));
            }

            public Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0)
            {
                Action<IDomainEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.Initiator, e.OccurrenceDate)(e.Fact, e.OccurrenceDate);
                return AddHadlerAction(new InternalEventHandler<IDomainEvent<TEntity, TFact>>(modSubscriber, e => EventFilters.All(f => f(e)), priority));
            }

        }

        public interface Init<out TEntity> : All<TEntity>, SubscriptionSyntax.All<IDomainEvent<TEntity>> where TEntity : IEntity
        {
        }
        
        internal class InitImpl<TEntity> : Init<TEntity> where TEntity : IEntity
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;

            public InitImpl(Func<IInternalEventHandler, Action> addHadlerAction)
            {
                this.addHadlerAction = addHadlerAction;
            }

            Or All<TEntity>.Or
            {
                get { return new OrImpl(addHadlerAction, new[] {typeof (IDomainEvent<TEntity>)}); }
            }

            Full<TEntity, TFact> All<TEntity>.DueTo<TFact>()
            {
                return new FullImpl<TEntity, TFact>(addHadlerAction);
            }

            SubscriptionSyntax.All<IDomainEvent<TEntity>> SubscriptionSyntax.All<IDomainEvent<TEntity>>.Where
                (Func<IDomainEvent<TEntity>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IDomainEvent<TEntity>, TProjection> SubscriptionSyntax.All<IDomainEvent<TEntity>>.Select<TProjection>
                (Func<IDomainEvent<TEntity>, TProjection> projection)
            {
                return new SubscriptionSyntax.ProjectionImpl<IDomainEvent<TEntity>, TProjection>
                    (new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IDomainEvent<TEntity>>.Subscribe(Action<IDomainEvent<TEntity>> subscriber,
                                                                           int priority)
            {
                Predicate<IDomainEvent<TEntity>> filter = e => true;
                return addHadlerAction(new InternalEventHandler<IDomainEvent<TEntity>>(subscriber, filter, priority));
            }

            Action SubscriptionSyntax.All<IDomainEvent<TEntity>>.Subscribe(IHandler<IDomainEvent<TEntity>> subscriber, int priority)
            {
                return (this as SubscriptionSyntax.All<IDomainEvent<TEntity>>).Subscribe(subscriber.Handle, priority);
            }
        }

        public interface Or : IHiddenObjectMembers
        {
            All InitiatedBy<TEntity>() where TEntity : IEntity;
        }

        internal class OrImpl : Or
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Type> initiatorTypes = new List<Type>();

            public OrImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> initiatorTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.initiatorTypes.AddRange(initiatorTypes);
            }

            All Or.InitiatedBy<TEntity>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(initiatorTypes) {typeof (IDomainEvent<TEntity>)});
            } 
        }

        // ReSharper restore InconsistentNaming         
    }
} 