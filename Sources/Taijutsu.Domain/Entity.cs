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
using System.Globalization;
using System.Reflection;
using Taijutsu.Domain.Event;
using Taijutsu.Domain.Event.Internal;
using EventAggregator = Taijutsu.Domain.Event.EventAggregator;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class Entity : IdentifiedObject<object>, IEntity
    {
        protected void Publish<TFact>(TFact fact) where TFact : IFact
        {
            EventAggregator.Raise(EventFor(fact));
        }

        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivators.ActivatorFor(InternalGetType(), typeof (TFact)).CreateInstance(this, fact);
        }
    }

    [Serializable]
    public abstract class Entity<TKey> : IdentifiedObject<TKey>, IEntity<TKey>, IEquatable<Entity<TKey>>
    {
        protected TKey entityKey;

        #region IEntity<TKey> Members

        public virtual TKey Key
        {
            get { return entityKey; }
        }

        public override bool Equals(object other)
        {
            var asEntity = (other as Entity<TKey>);

            return !ReferenceEquals(asEntity, null)
                   && InternalGetType() == asEntity.InternalGetType()
                   && Equals(asEntity as IdentifiedObject<TKey>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return entityKey.Equals(default(TKey)) ? string.Empty : entityKey.ToString();
        }

        #endregion

        #region IEquatable<Entity<TKey>> Members

        public virtual bool Equals(Entity<TKey> other)
        {
            if (ReferenceEquals(other, null)) return false;

            return InternalGetType() == other.InternalGetType() && Equals(other as IdentifiedObject<TKey>);
        }

        #endregion

        protected override TKey BuildIdentity()
        {
            return Key;
        }

        protected void Publish<TFact>(TFact fact) where TFact : IFact
        {
            EventAggregator.Raise(EventFor(fact));
        }

        protected virtual IDomainEvent EventFor<TFact>(TFact fact) where TFact : IFact
        {
            return DomainEventActivators.ActivatorFor(InternalGetType(), typeof(TFact)).CreateInstance(this, fact);
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