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
using System.ComponentModel;
using Taijutsu.Domain.Event;
using Taijutsu.Domain.Event.Internal;
using Taijutsu.Domain.Event.Syntax.Publishing;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class Entity : IdentifiableObject<object>, IEntity
    {
        protected static IEventStream OnStream
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivatorsHolder.ActivatorFor(InternalGetType(), typeof (TFact)).CreateInstance(this, fact, SeqGuid.NewGuid());
        }
    }

    [Serializable]
    public abstract class Entity<TId> : IdentifiableObject<TId>, IEntity<TId>, IEquatable<Entity<TId>>
    {
        protected TId id;

        public virtual TId Id
        {
            get { return id; }
            protected set { id = value; }
        }

        public override bool Equals(object other)
        {
            var asEntity = (other as Entity<TId>);

            return !ReferenceEquals(asEntity, null)
                   && InternalGetType() == asEntity.InternalGetType()
                   && Equals(asEntity as IdentifiableObject<TId>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return id.Equals(default(TId)) ? string.Empty : id.ToString();
        }

        public virtual bool Equals(Entity<TId> other)
        {
            if (ReferenceEquals(other, null)) return false;

            return InternalGetType() == other.InternalGetType() && Equals(other as IdentifiableObject<TId>);
        }

        protected override TId BuildIdentity()
        {
            return Id;
        }

        protected static IEventStream OnStream
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivatorsHolder.ActivatorFor(InternalGetType(), typeof (TFact)).CreateInstance(this, fact, SeqGuid.NewGuid());
        }

        public static bool operator ==(Entity<TId> left, Entity<TId> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        }

        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return !(left == right);
        }
    }
}