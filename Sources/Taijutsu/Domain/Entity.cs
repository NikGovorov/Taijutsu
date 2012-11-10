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
using Taijutsu.Domain.Event;
using Taijutsu.Domain.Event.Internal;
using Taijutsu.Domain.Event.Syntax.Publishing;
using Taijutsu.Domain.Event.Syntax.Subscribing;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class Entity : IdentifiableObject<object>, IEntity
    {
        protected static IObservableSyntax OnStream
        {
            get { return EventAggregator.OnStream; }
        }

        protected void Publish<TDomainEvent>(TDomainEvent ev) where TDomainEvent : IDomainEvent
        {
            EventAggregator.Publish(ev);
        }

        protected PublishingSyntax.Prepared DueTo<TFact>(TFact fact) where TFact : IFact
        {
            var ev = EventFor(fact);
            return new PublishingSyntax.PreparedImpl(() => Publish(ev));
        }

        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivatorsHolder.ActivatorFor(InternalGetType(),
                                                            typeof (TFact)).CreateInstance(this, fact, SeqGuid.NewGuid());
        }
    }

    [Serializable]
    public abstract class Entity<TKey> : IdentifiableObject<TKey>, IEntity<TKey>, IEquatable<Entity<TKey>>
    {
        protected TKey entityKey;


        public virtual TKey Key
        {
            get { return entityKey; }
            protected set { entityKey = value; }
        }

        public override bool Equals(object other)
        {
            var asEntity = (other as Entity<TKey>);

            return !ReferenceEquals(asEntity, null)
                   && InternalGetType() == asEntity.InternalGetType()
                   && Equals(asEntity as IdentifiableObject<TKey>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return entityKey.Equals(default(TKey)) ? string.Empty : entityKey.ToString();
        }

        public virtual bool Equals(Entity<TKey> other)
        {
            if (ReferenceEquals(other, null)) return false;

            return InternalGetType() == other.InternalGetType() && Equals(other as IdentifiableObject<TKey>);
        }

        protected override TKey BuildIdentity()
        {
            return Key;
        }


        protected static IObservableSyntax OnStream
        {
            get { return EventAggregator.OnStream; }
        }


        protected void Publish<TDomainEvent>(TDomainEvent ev) where TDomainEvent : IDomainEvent
        {
            EventAggregator.Publish(ev);
        }

        protected PublishingSyntax.Prepared DueTo<TFact>(TFact fact) where TFact : IFact
        {
            var ev = EventFor(fact);
            return new PublishingSyntax.PreparedImpl(() => Publish(ev));
        }

        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivatorsHolder.ActivatorFor(InternalGetType(),
                                                            typeof (TFact)).CreateInstance(this, fact, SeqGuid.NewGuid());
        }

        public static bool operator ==(Entity<TKey> left, Entity<TKey> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        }

        public static bool operator !=(Entity<TKey> left, Entity<TKey> right)
        {
            return !(left == right);
        }
    }
}