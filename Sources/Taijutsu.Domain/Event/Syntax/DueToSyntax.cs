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
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax
{
    public static class DueToSyntax
    {
        // ReSharper disable InconsistentNaming

        #region Nested type: All

        public interface All : IHideObjectMembers
        {
            Or Or { get; }
            SubscriptionSyntax.All<IDomainEvent<TEntity>> InitiatedBy<TEntity>() where TEntity : IEntity;
            SubscriptionSyntax.All<IExternalEvent<TEntity>> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        public interface All<out TFact> : IHideObjectMembers where TFact : IFact
        {
            Or Or { get; }
            InitiatedBySyntax.Full<TEntity, TFact> InitiatedBy<TEntity>() where TEntity : IEntity;
            AddressedToSyntax.Full<TEntity, TFact> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        #endregion

        #region Nested type: AllImpl

        internal class AllImpl : All
        {
             private readonly List<Type> factTypes = new List<Type>();
             private readonly Func<IEventHandler, Action> addHadlerAction;


            public AllImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> factTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.factTypes.AddRange(factTypes);
            }

            #region All Members

            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(factTypes)); }
            }

            SubscriptionSyntax.All<IDomainEvent<TEntity>> All.InitiatedBy<TEntity>()
            {
                return new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>(addHadlerAction, factTypes.Select<Type, Func<IDomainEvent<TEntity>, bool>>(t=>e=>t.IsAssignableFrom(e.GetType())));
            }

            SubscriptionSyntax.All<IExternalEvent<TEntity>> All.AddressedTo<TEntity>()
            {
                return new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>(addHadlerAction, factTypes.Select<Type, Func<IExternalEvent<TEntity>, bool>>(t => e => t.IsAssignableFrom(e.GetType())));
            }

            #endregion
        }

        #endregion

        #region Nested type: Init

        public interface Init<out TFact> : All<TFact>, SubscriptionSyntax.All<IEventDueToFact<TFact>>
            where TFact : IFact
        {
        }

        #endregion

        #region Nested type: InitImpl

        internal class InitImpl<TFact> : Init<TFact> where TFact : IFact
        {
            private readonly Func<IEventHandler, Action> addHadlerAction;

            public InitImpl(Func<IEventHandler, Action> addHadlerAction)
            {
                this.addHadlerAction = addHadlerAction;
            }

            #region Init<TFact> Members

            Or All<TFact>.Or
            {
                get { return new OrImpl(addHadlerAction, new[] { typeof(IEventDueToFact<TFact>) }); }
            }

            InitiatedBySyntax.Full<TEntity, TFact> All<TFact>.InitiatedBy<TEntity>()
            {
                return new InitiatedBySyntax.FullImpl<TEntity, TFact>(addHadlerAction);
            }

            AddressedToSyntax.Full<TEntity, TFact> All<TFact>.AddressedTo<TEntity>()
            {
                return new AddressedToSyntax.FullImpl<TEntity, TFact>(addHadlerAction);
            }

            SubscriptionSyntax.All<IEventDueToFact<TFact>> SubscriptionSyntax.All<IEventDueToFact<TFact>>.Where(Func<IEventDueToFact<TFact>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IEventDueToFact<TFact>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IEventDueToFact<TFact>, TProjection> SubscriptionSyntax.All<IEventDueToFact<TFact>>.Select<TProjection>(
                Func<IEventDueToFact<TFact>, TProjection> projection)
            {
                return
                    new SubscriptionSyntax.ProjectionImpl<IEventDueToFact<TFact>, TProjection>(
                        new SubscriptionSyntax.AllImpl<IEventDueToFact<TFact>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IEventDueToFact<TFact>>.Subscribe(Action<IEventDueToFact<TFact>> subscriber, int priority)
            {
                Predicate<IEventDueToFact<TFact>> filter = e => true;
                return addHadlerAction(new Internal.EventHandler<IEventDueToFact<TFact>>(subscriber, filter, priority));
            }

            #endregion
        }

        #endregion

        #region Nested type: Or

        public interface Or : IHideObjectMembers
        {
            All DueTo<TFact>() where TFact : IFact;
        }

        #endregion

        #region Nested type: OrImpl

        internal class OrImpl : Or
        {
            private readonly List<Type> factTypes = new List<Type>();
            private readonly Func<IEventHandler, Action> addHadlerAction;

            public OrImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> factTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.factTypes.AddRange(factTypes);
            }

            #region Or Members

            All Or.DueTo<TFact>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(factTypes) { typeof(IEventDueToFact<TFact>) });
            }

            #endregion
        }

        #endregion

        // ReSharper restore InconsistentNaming   
    }
}