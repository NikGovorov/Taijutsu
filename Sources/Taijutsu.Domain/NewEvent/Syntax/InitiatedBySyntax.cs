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

namespace Taijutsu.Domain.NewEvent.Syntax
{
    public static class InitiatedBySyntax
    {
        // ReSharper disable InconsistentNaming

        #region Nested type: All

        public interface All : IHideObjectMembers
        {
            Or Or { get; }
            SubscriptionSyntax.All<IEventDueToFact<TFact>> DueTo<TFact>() where TFact : IFact;
        }

        public interface All<out TEntity> : IHideObjectMembers where TEntity : IEntity
        {
            Or Or { get; }
            SubscriptionSyntax.All<IDomainEvent<TEntity, TFact>> DueTo<TFact>() where TFact : IFact;
        }

        #endregion

        internal class AllImpl : All
        {
            private readonly List<Type> initiatorsTypes = new List<Type>();
            private readonly Func<IEventHandler, Action> addHadlerAction;

            public AllImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> initiatorsTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.initiatorsTypes.AddRange(initiatorsTypes);
            }

            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(initiatorsTypes)); }
            }

            SubscriptionSyntax.All<IEventDueToFact<TFact>> All.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IEventDueToFact<TFact>>(addHadlerAction, initiatorsTypes.Select<Type, Func<IEventDueToFact<TFact>, bool>>(t => e => t.IsAssignableFrom(e.GetType())));
            }
        }


        #region Nested type: Init

        public interface Init<out TEntity> : All<TEntity>, SubscriptionSyntax.All<IDomainEvent<TEntity>>
            where TEntity : IEntity
        {
        }

        #endregion

        #region Nested type: InitImpl

        internal class InitImpl<TEntity> : Init<TEntity> where TEntity : IEntity
        {
            private readonly Func<IEventHandler, Action> addHadlerAction;

            public InitImpl(Func<IEventHandler, Action> addHadlerAction)
            {
                this.addHadlerAction = addHadlerAction;
            }

            #region Init<TEntity> Members

            Or All<TEntity>.Or
            {
                get { return new OrImpl(addHadlerAction, new[] { typeof(IDomainEvent<TEntity>) }); }
            }

            SubscriptionSyntax.All<IDomainEvent<TEntity, TFact>> All<TEntity>.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity, TFact>>(addHadlerAction);
            }

            SubscriptionSyntax.All<IDomainEvent<TEntity>> SubscriptionSyntax.All<IDomainEvent<TEntity>>.Where(Func<IDomainEvent<TEntity>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IDomainEvent<TEntity>, TProjection> SubscriptionSyntax.All<IDomainEvent<TEntity>>.Select<TProjection>(
                Func<IDomainEvent<TEntity>, TProjection> projection)
            {
                return
                    new SubscriptionSyntax.ProjectionImpl<IDomainEvent<TEntity>, TProjection>(
                        new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IDomainEvent<TEntity>>.Subscribe(Action<IDomainEvent<TEntity>> subscriber, int priority)
            {
                Predicate<IDomainEvent<TEntity>> filter = e => true;
                return addHadlerAction(new EventHandler<IDomainEvent<TEntity>>(subscriber, filter, priority));
            }

            #endregion
        }

        #endregion

        #region Nested type: Or

        public interface Or : IHideObjectMembers
        {
            All InitiatedBy<TEntity>() where TEntity : IEntity;
        }

        #endregion

        internal class OrImpl : Or
        {
            private readonly List<Type> initiatorsTypes = new List<Type>();
            private readonly Func<IEventHandler, Action> addHadlerAction;

            public OrImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> initiatorsTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.initiatorsTypes.AddRange(initiatorsTypes);
            }

            All Or.InitiatedBy<TEntity>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(initiatorsTypes) { typeof(IDomainEvent<TEntity>) });
            }
        }

        // ReSharper restore InconsistentNaming         
    }
}