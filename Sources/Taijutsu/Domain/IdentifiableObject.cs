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
namespace Taijutsu.Domain
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public abstract class IdentifiableObject<TIdentifier> : IEquatable<IdentifiableObject<TIdentifier>>
    {
        [NonSerialized]
        private int? hashCode;

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", 
            Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public virtual bool Equals(IdentifiableObject<TIdentifier> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (Equals(this.BuildIdentity(), default(TIdentifier))
                && Equals(other.BuildIdentity(), default(TIdentifier)))
            {
                return ReferenceEquals(other, this);
            }

            return ReferenceEquals(other, this) || Equals(other.BuildIdentity(), this.BuildIdentity());
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IdentifiableObject<TIdentifier>);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", 
            Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            if (this.hashCode.HasValue)
            {
                return this.hashCode.Value;
            }

            if (Equals(this.BuildIdentity(), default(TIdentifier)))
            {
                // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
                this.hashCode = base.GetHashCode();

                // ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
                return this.hashCode.Value;
            }

            // ReSharper restore NonReadonlyFieldInGetHashCode
            return this.BuildIdentity().GetHashCode();
        }

        protected abstract TIdentifier BuildIdentity();

        protected virtual Type InternalGetType()
        {
            return this.GetType();
        }
    }
}