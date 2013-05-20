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
    using System.Collections.Generic;

    public interface ISpecification : IHiddenObjectMembers
    {
        bool IsSatisfiedBy(object candidate);
    }

    public interface ISpecification<TDomainObject> : ISpecification
        where TDomainObject : IDomainObject
    {
        bool IsSatisfiedBy(TDomainObject candidate);

        IEnumerable<TDomainObject> SatisfyingElementsFrom(IEnumerable<TDomainObject> candidates);

        ISpecification<TDomainObject> And(ISpecification<TDomainObject> and);

        ISpecification<TDomainObject> Or(ISpecification<TDomainObject> or);

        ISpecification<TDomainObject> Not();
    }
}