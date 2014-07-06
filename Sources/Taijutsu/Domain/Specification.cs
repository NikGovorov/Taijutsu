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
using System.Collections.Generic;
using System.Linq;

using Taijutsu.Annotation;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class Specification<TDomainObject> : ISpecification<TDomainObject>
        where TDomainObject : IDomainObject
    {
        public static Specification<TDomainObject> operator &([NotNull] Specification<TDomainObject> one, [NotNull] Specification<TDomainObject> other)
        {
            if (one == null)
            {
                throw new ArgumentNullException("one");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return one.And(other);
        }

        public static Specification<TDomainObject> operator |([NotNull] Specification<TDomainObject> one, [NotNull] Specification<TDomainObject> other)
        {
            if (one == null)
            {
                throw new ArgumentNullException("one");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return one.Or(other);
        }

        public static Specification<TDomainObject> operator !([NotNull] Specification<TDomainObject> specification)
        {
            if (specification == null)
            {
                throw new ArgumentNullException("specification");
            }

            return specification.Not();
        }

        public virtual bool IsSatisfiedBy(object candidate)
        {
            return candidate is TDomainObject && SatisfyingElementsFrom(new[] { (TDomainObject)candidate }).Any();
        }

        public virtual bool IsSatisfiedBy(TDomainObject candidate)
        {
            return SatisfyingElementsFrom(new[] { candidate }).Any();
        }

        public abstract IEnumerable<TDomainObject> SatisfyingElementsFrom(IEnumerable<TDomainObject> candidates);

        ISpecification<TDomainObject> ISpecification<TDomainObject>.And(ISpecification<TDomainObject> other)
        {
            return And(other);
        }

        ISpecification<TDomainObject> ISpecification<TDomainObject>.Or(ISpecification<TDomainObject> other)
        {
            return Or(other);
        }

        ISpecification<TDomainObject> ISpecification<TDomainObject>.Not()
        {
            return Not();
        }

        public virtual Specification<TDomainObject> And([NotNull] ISpecification<TDomainObject> other)
        {
            return new AndSpecification<TDomainObject>(this, other);
        }

        public virtual Specification<TDomainObject> Or([NotNull] ISpecification<TDomainObject> other)
        {
            return new OrSpecification<TDomainObject>(this, other);
        }

        public virtual Specification<TDomainObject> Not()
        {
            return new NotSpecification<TDomainObject>(this);
        }
    }
}