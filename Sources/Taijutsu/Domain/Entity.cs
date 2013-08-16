// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Domain.Event;
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain
{
    [PublicApi]
    [Serializable]
    public abstract class Entity : IdentifiableObject<object>, IEntity
    {
        protected static IEventStream OnStream
        {
            get { return EventAggregator.OnStream; }
        }

        protected static SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return OnStream.Of<TEvent>();
        }

        protected static void Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        protected void Publish<TDomainEvent>(TDomainEvent ev) where TDomainEvent : IDomainEvent
        {
            EventAggregator.Publish(ev);
        }
    }

    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppress the warning for generics.")]
    public abstract class Entity<TId> : IdentifiableObject<TId>, IEntity<TId>, IEquatable<Entity<TId>>
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Can be used in inhertor's constructor.")]
        protected TId id;

        public virtual TId Id
        {
            get { return id; }

            protected set { id = value; }
        }

        protected static IEventStream OnStream
        {
            get { return EventAggregator.OnStream; }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public static bool operator ==(Entity<TId> left, Entity<TId> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        }

        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return !(left == right);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public override bool Equals(object other)
        {
            var asEntity = other as Entity<TId>;

            return !ReferenceEquals(asEntity, null) && InternalGetType() == asEntity.InternalGetType() && Equals(asEntity as IdentifiableObject<TId>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return id.Equals(default(TId)) ? string.Empty : id.ToString();
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public virtual bool Equals(Entity<TId> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return InternalGetType() == other.InternalGetType() && Equals(other as IdentifiableObject<TId>);
        }

        protected static SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return OnStream.Of<TEvent>();
        }

        protected static void Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        protected void Publish<TDomainEvent>(TDomainEvent ev) where TDomainEvent : IDomainEvent
        {
            EventAggregator.Publish(ev);
        }

        protected override TId BuildIdentity()
        {
            return Id;
        }
    }
}