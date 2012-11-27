#region License

// Copyright 2009-2013 Nikita Govorov
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
    public static class DueToSyntax
    {
        // ReSharper disable InconsistentNaming

        public interface All : IHiddenObjectMembers
        {
            Or Or { get; }
            SubscriptionSyntax.All<IDomainEvent<TEntity>> InitiatedBy<TEntity>() where TEntity : IEntity;
            SubscriptionSyntax.All<IExternalEvent<TEntity>> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        public interface All<out TFact> : IHiddenObjectMembers where TFact : IFact
        {
            Or Or { get; }
            InitiatedBySyntax.Full<TEntity, TFact> InitiatedBy<TEntity>() where TEntity : IEntity;
            AddressedToSyntax.Full<TEntity, TFact> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        internal class AllImpl : All
        {
             private readonly List<Type> factTypes = new List<Type>();
             private readonly Func<IInternalEventHandler, Action> addHadlerAction;


            public AllImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> factTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.factTypes.AddRange(factTypes);
            }

            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(factTypes)); }
            }

            SubscriptionSyntax.All<IDomainEvent<TEntity>> All.InitiatedBy<TEntity>()
            {
                return new SubscriptionSyntax.AllImpl<IDomainEvent<TEntity>>
                    (addHadlerAction, factTypes.Select<Type, Func<IDomainEvent<TEntity>, bool>>(t=>e=>t.IsInstanceOfType(e)));
            }

            SubscriptionSyntax.All<IExternalEvent<TEntity>> All.AddressedTo<TEntity>()
            {
                return new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>
                    (addHadlerAction, factTypes.Select<Type, Func<IExternalEvent<TEntity>, bool>>(t => e => t.IsInstanceOfType(e)));
            }
        }

        public interface Init<out TFact> : All<TFact>, SubscriptionSyntax.All<IFactEvent<TFact>>
            where TFact : IFact
        {
        }
        
        internal class InitImpl<TFact> : Init<TFact> where TFact : IFact
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;

            public InitImpl(Func<IInternalEventHandler, Action> addHadlerAction)
            {
                this.addHadlerAction = addHadlerAction;
            }

            Or All<TFact>.Or
            {
                get { return new OrImpl(addHadlerAction, new[] { typeof(IFactEvent<TFact>) }); }
            }

            InitiatedBySyntax.Full<TEntity, TFact> All<TFact>.InitiatedBy<TEntity>()
            {
                return new InitiatedBySyntax.FullImpl<TEntity, TFact>(addHadlerAction);
            }

            AddressedToSyntax.Full<TEntity, TFact> All<TFact>.AddressedTo<TEntity>()
            {
                return new AddressedToSyntax.FullImpl<TEntity, TFact>(addHadlerAction);
            }

            SubscriptionSyntax.All<IFactEvent<TFact>> SubscriptionSyntax.All<IFactEvent<TFact>>.Where
                (Func<IFactEvent<TFact>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IFactEvent<TFact>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IFactEvent<TFact>, TProjection> SubscriptionSyntax.All<IFactEvent<TFact>>.Select<TProjection>
                (Func<IFactEvent<TFact>, TProjection> projection)
            {
                return new SubscriptionSyntax.ProjectionImpl<IFactEvent<TFact>, TProjection>
                    (new SubscriptionSyntax.AllImpl<IFactEvent<TFact>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IFactEvent<TFact>>.Subscribe(Action<IFactEvent<TFact>> subscriber, int priority)
            {
                Predicate<IFactEvent<TFact>> filter = e => true;
                return addHadlerAction(new InternalEventHandler<IFactEvent<TFact>>(subscriber, filter, priority));
            }

            Action SubscriptionSyntax.All<IFactEvent<TFact>>.Subscribe(IHandler<IFactEvent<TFact>> subscriber, int priority)
            {
                return (this as SubscriptionSyntax.All<IFactEvent<TFact>>).Subscribe(subscriber.Handle, priority);
            }
        }

        public interface Or : IHiddenObjectMembers
        {
            All DueTo<TFact>() where TFact : IFact;
        }

        internal class OrImpl : Or
        {
            private readonly List<Type> factTypes = new List<Type>();
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;

            public OrImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> factTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.factTypes.AddRange(factTypes);
            }

            All Or.DueTo<TFact>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(factTypes) { typeof(IFactEvent<TFact>) });
            }
        }

        // ReSharper restore InconsistentNaming   
    }
}