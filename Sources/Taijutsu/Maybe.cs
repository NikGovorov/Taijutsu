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

namespace Taijutsu
{
    
    public interface IMaybe<out T> : IHideObjectMembers
    {
        T Value { get; }
        bool HasValue { get; }
    }

    [Serializable]
    public class Maybe<T> : IMaybe<T>
    {
        // ReSharper disable StaticFieldInGenericType
        public static readonly Maybe<T> Empty = new Maybe<T>();
        // ReSharper restore StaticFieldInGenericType

        protected readonly T val;

        protected Maybe()
        {
        }

        public Maybe(T value)
        {
            val = value;
        }

        public virtual T Value
        {
            get
            {
                AssertNotNullValue();
                return val;
            }
        }

        public virtual bool HasValue
        {
            get { return !Equals(val, default(T)); }
        }



        public virtual Maybe<T> Apply(Action<T> action)
        {
            if (HasValue)
            {
                action(Value);
            }
            return this;
        }

        public virtual Maybe<T> Handle(Action action)
        {
            if (!HasValue)
            {
                action();
            }

            return this;
        }

        public override string ToString()
        {
            return HasValue ? val.ToString() : string.Format("Empty Maybe of {0}.", typeof (T));
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

        protected virtual void AssertNotNullValue()
        {
            if (!HasValue)
                throw new InvalidOperationException(string.Format("Maybe of {0} must have value.", typeof (T)));
        }
    }

    public static class MaybeEx
    {
        public static Maybe<T> Maybe<T>(this T value)
        {
            return new Maybe<T>(value);
        }
    }
}