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
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class OrSpecification<TDomainObject> : CompositeSpecification<TDomainObject>
        where TDomainObject : IDomainObject
    {
        public OrSpecification(ISpecification<TDomainObject> one, ISpecification<TDomainObject> other)
            : base(one, other)
        {
        }

        public override bool IsSatisfiedBy(object candidate)
        {
            return this.One.IsSatisfiedBy(candidate) || this.Other.IsSatisfiedBy(candidate);
        }

        public override IEnumerable<TDomainObject> SatisfyingElementsFrom(IEnumerable<TDomainObject> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException("candidates");
            }

            candidates = candidates.ToList();

            return this.One.SatisfyingElementsFrom(candidates).Union(this.Other.SatisfyingElementsFrom(candidates));
        }
    }
}