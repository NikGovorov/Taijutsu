// Copyright 2009-2014 Nikita Govorov
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

namespace Taijutsu
{
    [Serializable]
    public sealed class Maybe<T> : IMaybe<T>
    {
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T));

        private readonly T value;

        public Maybe(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                AssertNotNullValue();
                return value;
            }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public bool HasValue
        {
            get { return !Equals(value, default(T)); }
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static implicit operator bool(Maybe<T> maybe)
        {
            return maybe.HasValue;
        }

        public static explicit operator T(Maybe<T> maybe)
        {
            maybe.AssertNotNullValue();
            return maybe.Value;
        }

        public override string ToString()
        {
            return HasValue ? value.ToString() : string.Format("Empty Maybe of {0}.", typeof(T));
        }

        private void AssertNotNullValue()
        {
            if (!HasValue)
            {
                throw new InvalidOperationException(string.Format("Maybe of {0} must have value.", typeof(T)));
            }
        }
    }
}