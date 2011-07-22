﻿// Copyright 2009-2011 Taijutsu.
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

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class Entity : IdentifiedObject<object>, IEntity
    {
    }

    [Serializable]
    public abstract class Entity<TKey> : IdentifiedObject<TKey>, IEntity<TKey>, IEquatable<Entity<TKey>>
    {
        protected TKey entityKey;

        #region IEntity<TKey> Members

        public virtual TKey Key
        {
            get { return entityKey; }
            protected set { entityKey = value; }
        }

        #endregion

        #region IEquatable<Entity<TKey>> Members

        public virtual bool Equals(Entity<TKey> other)
        {
            return Equals(other as IdentifiedObject<TKey>);
        }

        #endregion

        public override bool Equals(object other)
        {
            return base.Equals(other as IdentifiedObject<TKey>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected override TKey BuildIdentity()
        {
            return Key;
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