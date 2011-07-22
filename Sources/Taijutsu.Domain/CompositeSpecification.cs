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
    internal abstract class CompositeSpecification<TDomainObject> : Specification<TDomainObject> where TDomainObject : IDomainObject
    {
        private ISpecification<TDomainObject> one;
        private ISpecification<TDomainObject> other;

        protected CompositeSpecification(ISpecification<TDomainObject> one, ISpecification<TDomainObject> other)
        {
            this.one = one;
            this.other = other;
        }

        public virtual ISpecification<TDomainObject> One
        {
            get { return one; }
            protected set { one = value; }
        }

        public virtual ISpecification<TDomainObject> Other
        {
            get { return other; }
            protected set { other = value; }
        }
    }
}