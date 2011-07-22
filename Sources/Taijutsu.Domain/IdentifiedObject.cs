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

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class IdentifiedObject<TIdentifier> : IEquatable<IdentifiedObject<TIdentifier>>
    {
        [NonSerialized] private int? hashCode;

        #region IEquatable<IdentifiedObject<TIdentifier>> Members

        public virtual bool Equals(IdentifiedObject<TIdentifier> other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (Equals(BuildIdentity(), default(TIdentifier)) && Equals(other.BuildIdentity(), default(TIdentifier)))
            {
                return ReferenceEquals(other, this);
            }
            return ReferenceEquals(other, this) || Equals(other.BuildIdentity(), BuildIdentity());
        }

        #endregion

        protected abstract TIdentifier BuildIdentity();

        public override bool Equals(object obj)
        {
            return Equals(obj as IdentifiedObject<TIdentifier>);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            if (hashCode.HasValue)
                return hashCode.Value;

            if (Equals(BuildIdentity(), default(TIdentifier)))
            {
                // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
                hashCode = base.GetHashCode();
                // ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
                return hashCode.Value;
            }

            // ReSharper restore NonReadonlyFieldInGetHashCode
            return BuildIdentity().GetHashCode();
        }

        public static bool operator ==(
            IdentifiedObject<TIdentifier> left, IdentifiedObject<TIdentifier> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        }

        public static bool operator !=(
            IdentifiedObject<TIdentifier> left, IdentifiedObject<TIdentifier> right)
        {
            return !(left == right);
        }
    }
}