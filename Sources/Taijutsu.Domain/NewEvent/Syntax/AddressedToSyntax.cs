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
    public static class AddressedToSyntax
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
            SubscriptionSyntax.All<IExternalEvent<TEntity, TFact>> DueTo<TFact>() where TFact : IFact;
        }

        #endregion

        #region Nested type: AllImpl

        internal class AllImpl : All
        {
            private readonly Func<IEventHandler, Action> addHadlerAction;
            private readonly List<Type> addressTypes = new List<Type>();

            public AllImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> addressTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.addressTypes.AddRange(addressTypes);
            }

            #region All Members

            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(addressTypes)); }
            }

            SubscriptionSyntax.All<IEventDueToFact<TFact>> All.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IEventDueToFact<TFact>>(addHadlerAction,
                                                                              addressTypes.Select
                                                                                  <Type,
                                                                                  Func<IEventDueToFact<TFact>, bool>>(
                                                                                      t =>
                                                                                      e =>
                                                                                      t.IsAssignableFrom(e.GetType())));
            }

            #endregion
        }

        #endregion

        #region Nested type: Init

        public interface Init<out TEntity> : All<TEntity>,
                                             SubscriptionSyntax.All<IExternalEvent<TEntity>>
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
                get { return new OrImpl(addHadlerAction, new[] {typeof (IExternalEvent<TEntity>)}); }
            }

            SubscriptionSyntax.All<IExternalEvent<TEntity, TFact>> All<TEntity>.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity, TFact>>(addHadlerAction);
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

            Action SubscriptionSyntax.All<IExternalEvent<TEntity>>.Subscribe(Action<IExternalEvent<TEntity>> subscriber,
                                                                             int priority)
            {
                Predicate<IExternalEvent<TEntity>> filter = e => true;
                return addHadlerAction(new EventHandler<IExternalEvent<TEntity>>(subscriber, filter, priority));
            }

            #endregion
        }

        #endregion

        #region Nested type: Or

        public interface Or : IHideObjectMembers
        {
            All AddressedTo<TEntity>() where TEntity : IEntity;
        }

        #endregion

        #region Nested type: OrImpl

        internal class OrImpl : Or
        {
            private readonly Func<IEventHandler, Action> addHadlerAction;
            private readonly List<Type> addressTypes = new List<Type>();

            public OrImpl(Func<IEventHandler, Action> addHadlerAction, IEnumerable<Type> addressTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.addressTypes.AddRange(addressTypes);
            }

            #region Or Members

            All Or.AddressedTo<TEntity>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(addressTypes) {typeof (IExternalEvent<TEntity>)});
            }

            #endregion
        }

        #endregion

        // ReSharper restore InconsistentNaming       
    }
}