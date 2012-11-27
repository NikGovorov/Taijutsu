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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Taijutsu.Domain;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class SpecificationFixture
    {
        private readonly User silverUser = new User {Balance = 100};
        private readonly User simpleUser = new User {Balance = 99};
        private readonly User goldUser = new User {Balance = 101};
        private readonly object notUser = new object();

        private readonly GoldUserSpecification goldUserSpec = new GoldUserSpecification();
        private readonly SilverUserSpecification silverUserSpec = new SilverUserSpecification();

        [Test]
        public virtual void IsSatisfiedByShouldUseSatisfyingElementsFrom()
        {
            Assert.False(goldUserSpec.IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.IsSatisfiedBy(simpleUser));
            Assert.False(goldUserSpec.IsSatisfiedBy(silverUser));
            Assert.True(goldUserSpec.IsSatisfiedBy(goldUser));
            Assert.True(silverUserSpec.IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void NotSpecShouldInvertResultOfIsSatisfiedByOfSourceSpec()
        {
            Assert.True(goldUserSpec.Not().IsSatisfiedBy(notUser));
            Assert.True(goldUserSpec.Not().IsSatisfiedBy(simpleUser));
            Assert.True(goldUserSpec.Not().IsSatisfiedBy(silverUser));
            Assert.False(goldUserSpec.Not().IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void NotSpecShouldInvertResultOfSatisfyingElementsFromOfSourceSpec()
        {
            var users = new[] {simpleUser, silverUser, goldUser};

            Assert.AreEqual(2, goldUserSpec.Not().SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.Not().SatisfyingElementsFrom(users).Any(u => u == simpleUser));
            Assert.True(((ISpecification<User>)goldUserSpec).Not().SatisfyingElementsFrom(users).Any(u => u == silverUser));
        }

        [Test]
        public virtual void NotOperatorShouldInvertResult()
        {
            Assert.True((!goldUserSpec).IsSatisfiedBy(notUser));
            Assert.True((!goldUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True((!goldUserSpec).IsSatisfiedBy(silverUser));
            Assert.False((!goldUserSpec).IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void OrSpecShouldCompareResultOfIsSatisfiedByOfSourceSpecsWithLogicalOr()
        {
            Assert.False(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True(((ISpecification<User>)goldUserSpec).Or(silverUserSpec).IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void OrSpecShouldCompareResultOfSatisfyingElementsFromOfSourceSpecsWithLogicalOr()
        {
            var users = new[] {simpleUser, silverUser, goldUser};

            Assert.AreEqual(2, goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Any(u => u == goldUser));
            Assert.True(goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Any(u => u == silverUser));
        }

        [Test]
        public virtual void OrOperatorShouldDoLogicalOrOverResult()
        {
            Assert.False((goldUserSpec | silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False((goldUserSpec | silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True((goldUserSpec | silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True((goldUserSpec | silverUserSpec).IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void AndSpecShouldCompareResultOfIsSatisfiedByOfSourceSpecsWithLogicalAnd()
        {
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True(((ISpecification<User>)goldUserSpec).And(silverUserSpec).IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void AndSpecShouldCompareResultOfSatisfyingElementsFromOfSourceSpecsWithLogicalAnd()
        {
            var users = new[] {simpleUser, silverUser, goldUser};

            Assert.AreEqual(1, goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Any(u => u == goldUser));
            Assert.False(goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Any(u => u == silverUser));
        }

        [Test]
        public virtual void AndOperatorShouldDoLogicalAndOverResult()
        {
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True((goldUserSpec & silverUserSpec).IsSatisfiedBy(goldUser));
        }

        [Test]
        public virtual void ComplexOperatorCombinationsShouldWorkWithIsSatisfiedBy()
        {
            Assert.True(((goldUserSpec & silverUserSpec) | (!silverUserSpec)).IsSatisfiedBy(simpleUser));
            Assert.True(((goldUserSpec & silverUserSpec) | (!goldUserSpec)).IsSatisfiedBy(notUser));
        }

        [Test]
        public virtual void ComplexOperatorCombinationsShouldWorkWithSatisfyingElementsFrom()
        {
            var users = new[] {simpleUser, silverUser, goldUser};
            Assert.AreEqual(3, ((goldUserSpec & silverUserSpec) | (!goldUserSpec)).SatisfyingElementsFrom(users).Count());
        }


        private class User : IDomainObject
        {
            public decimal Balance { get; set; }
        }

        private class GoldUserSpecification : Specification<User>
        {
            public override IEnumerable<User> SatisfyingElementsFrom(IEnumerable<User> candidates)
            {
                return candidates.Where(u => u.Balance > 100);
            }
        }

        private class SilverUserSpecification : Specification<User>
        {
            public override IEnumerable<User> SatisfyingElementsFrom(IEnumerable<User> candidates)
            {
                return candidates.Where(u => u.Balance >= 100);
            }
        }
    }
}