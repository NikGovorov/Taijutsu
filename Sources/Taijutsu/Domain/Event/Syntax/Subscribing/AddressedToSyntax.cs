#region License

//  Copyright 2009-2013 Nikita Govorov
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
    public static class AddressedToSyntax
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
            private readonly List<Type> recipientTypes = new List<Type>();

            public AllImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> recipientTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.recipientTypes.AddRange(recipientTypes);
            }

            
            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(recipientTypes)); }
            }

            SubscriptionSyntax.All<IFactEvent<TFact>> All.DueTo<TFact>()
            {
                Func<IFactEvent<TFact>, bool> typeFilter = ev => recipientTypes.Any(t => t.IsInstanceOfType(ev));

                return new SubscriptionSyntax.AllImpl<IFactEvent<TFact>>(addHadlerAction, new[] { typeFilter });
            }
        }

        public interface Full<out TEntity, out TFact> : SubscriptionSyntax.All<IExternalEvent<TEntity, TFact>>
            where TEntity : IEntity where TFact : IFact
        {
            Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0);

            Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0);

            Action Subscribe(Func<TEntity, DateTime, DateTime, Action<TFact, DateTime, DateTime>> subscriber, int priority = 0);
        }

        internal class FullImpl<TEntity, TFact> : SubscriptionSyntax.AllImpl<IExternalEvent<TEntity, TFact>>, Full<TEntity, TFact>
            where TEntity : IEntity where TFact : IFact
        {
            internal FullImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Func<IExternalEvent<TEntity, TFact>, bool>> eventFilters = null)
                : base(addHadlerAction, eventFilters)
            {
            }

            public Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.Recipient)(e.Fact);
                return AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e => EventFilters.All(f => f(e)), priority));
            }

            public Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.Recipient, e.OccurrenceDate)(e.Fact, e.OccurrenceDate);
                return AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e => EventFilters.All(f => f(e)), priority));
            }

            public Action Subscribe(Func<TEntity, DateTime, DateTime, Action<TFact, DateTime, DateTime>> subscriber, int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.Recipient, e.OccurrenceDate, e.NoticeDate)(e.Fact, e.OccurrenceDate, e.NoticeDate);

                return AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e => EventFilters.All(f => f(e)), priority));
            }

        }

        public interface Init<out TEntity> : All<TEntity>, SubscriptionSyntax.All<IExternalEvent<TEntity>>
            where TEntity : IEntity
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
                get { return new OrImpl(addHadlerAction, new[] {typeof (IExternalEvent<TEntity>)}); }
            }

            Full<TEntity, TFact> All<TEntity>.DueTo<TFact>()
            {
                return new FullImpl<TEntity, TFact>(addHadlerAction);
            }

            SubscriptionSyntax.All<IExternalEvent<TEntity>> SubscriptionSyntax.All<IExternalEvent<TEntity>>.Where(
                Func<IExternalEvent<TEntity>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IExternalEvent<TEntity>, TProjection>
                SubscriptionSyntax.All<IExternalEvent<TEntity>>.Select<TProjection>(
                Func<IExternalEvent<TEntity>, TProjection> projection)
            {
                return
                    new SubscriptionSyntax.ProjectionImpl<IExternalEvent<TEntity>, TProjection>(
                        new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IExternalEvent<TEntity>>.Subscribe(Action<IExternalEvent<TEntity>> subscriber, int priority)
            {
                Predicate<IExternalEvent<TEntity>> filter = e => true;
                return addHadlerAction(new InternalEventHandler<IExternalEvent<TEntity>>(subscriber, filter, priority));
            }

            Action SubscriptionSyntax.All<IExternalEvent<TEntity>>.Subscribe(
                IHandler<IExternalEvent<TEntity>> subscriber, int priority)
            {
                return (this as SubscriptionSyntax.All<IExternalEvent<TEntity>>).Subscribe(subscriber.Handle, priority);
            }

        }

        public interface Or : IHiddenObjectMembers
        {
            All AddressedTo<TEntity>() where TEntity : IEntity;
        }
      
        internal class OrImpl : Or
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Type> recipientTypes = new List<Type>();

            public OrImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> recipientTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.recipientTypes.AddRange(recipientTypes);
            }

            All Or.AddressedTo<TEntity>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(recipientTypes) {typeof (IExternalEvent<TEntity>)});
            }

        }

        // ReSharper restore InconsistentNaming       
    }
}