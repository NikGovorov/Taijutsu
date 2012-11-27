#region License

// Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class CompositeSpecification<TDomainObject> : Specification<TDomainObject>
        where TDomainObject : IDomainObject
    {
        protected ISpecification<TDomainObject> oneSpec;
        protected ISpecification<TDomainObject> otherSpec;

        protected CompositeSpecification(ISpecification<TDomainObject> one, ISpecification<TDomainObject> other)
        {
            oneSpec = one;
            otherSpec = other;
        }

        public virtual ISpecification<TDomainObject> One
        {
            get { return oneSpec; }
            protected set { oneSpec = value; }
        }

        public virtual ISpecification<TDomainObject> Other
        {
            get { return otherSpec; }
            protected set { otherSpec = value; }
        }
    }
}