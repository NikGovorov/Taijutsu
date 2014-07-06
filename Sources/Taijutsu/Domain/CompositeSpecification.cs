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

using Taijutsu.Annotation;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class CompositeSpecification<TDomainObject> : Specification<TDomainObject>, ICompositeSpecification<TDomainObject> where TDomainObject : IDomainObject
    {
        private ISpecification<TDomainObject> oneSpec;

        private ISpecification<TDomainObject> otherSpec;

        protected CompositeSpecification([NotNull] ISpecification<TDomainObject> one, [NotNull] ISpecification<TDomainObject> other)
        {
            if (one == null)
            {
                throw new ArgumentNullException("one");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            oneSpec = one;
            otherSpec = other;
        }

        protected CompositeSpecification()
        {
        }

        public ISpecification<TDomainObject> One
        {
            get { return oneSpec; }
            protected set { oneSpec = value; }
        }

        public ISpecification<TDomainObject> Other
        {
            get { return otherSpec; }
            protected set { otherSpec = value; }
        }
    }
}