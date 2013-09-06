﻿// Copyright 2009-2013 Nikita Govorov
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
    public class NotSpecification<TDomainObject> : Specification<TDomainObject>
        where TDomainObject : IDomainObject
    {
        private readonly ISpecification<TDomainObject> origin;

        public NotSpecification([NotNull] ISpecification<TDomainObject> origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException("origin");
            }

            this.origin = origin;
        }

        public override bool IsSatisfiedBy(object candidate)
        {
            return (!(candidate is TDomainObject)) || SatisfyingElementsFrom(new[] { (TDomainObject)candidate }).Any();
        }

        public override IEnumerable<TDomainObject> SatisfyingElementsFrom(IEnumerable<TDomainObject> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException("candidates");
            }

            candidates = candidates.ToList();

            return candidates.Except(origin.SatisfyingElementsFrom(candidates));
        }
    }
}