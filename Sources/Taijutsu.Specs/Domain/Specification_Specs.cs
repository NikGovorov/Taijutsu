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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Taijutsu.Domain;

namespace Taijutsu.Specs.Domain
{
// ReSharper disable InconsistentNaming
    [TestFixture]
    public class Specification_Specs
    {
        User silverUser = new User { Balance = 100 };
        User simpleUser = new User { Balance = 99 };
        User goldUser = new User { Balance = 101 };
        object notUser = new object();

        GoldUserSpecification goldUserSpec = new GoldUserSpecification();
        SilverUserSpecification silverUserSpec = new SilverUserSpecification();

        [Test]
        public virtual void IsSatisfiedBy_should_work_based_on_SatisfyingElementsFrom()
        {
           
            
            Assert.False(goldUserSpec.IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.IsSatisfiedBy(simpleUser));
            Assert.False(goldUserSpec.IsSatisfiedBy(silverUser));
            Assert.True(goldUserSpec.IsSatisfiedBy(goldUser));
            Assert.True(silverUserSpec.IsSatisfiedBy(goldUser));

        }

        [Test]
        public virtual void Not_specification_should_invert_result_of_IsSatisfiedBy_of_source_spec()
        {


            Assert.True(goldUserSpec.Not().IsSatisfiedBy(notUser));
            Assert.True(goldUserSpec.Not().IsSatisfiedBy(simpleUser));
            Assert.True(goldUserSpec.Not().IsSatisfiedBy(silverUser));
            Assert.False(goldUserSpec.Not().IsSatisfiedBy(goldUser));

        }

        [Test]
        public virtual void Not_specification_should_invert_result_of_SatisfyingElementsFrom_of_source_spec()
        {


            var users = new[] {simpleUser, silverUser, goldUser};

            Assert.AreEqual(2, goldUserSpec.Not().SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.Not().SatisfyingElementsFrom(users).Where(u => u == simpleUser).Any());
            Assert.True(goldUserSpec.Not().SatisfyingElementsFrom(users).Where(u => u == silverUser).Any());
        }


        [Test]
        public virtual void Not_specification_operator_overload_should_work()
        {


            Assert.True((!goldUserSpec).IsSatisfiedBy(notUser));
            Assert.True((!goldUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True((!goldUserSpec).IsSatisfiedBy(silverUser));
            Assert.False((!goldUserSpec).IsSatisfiedBy(goldUser));

        }


        [Test]
        public virtual void Or_specification_should_compare_result_of_IsSatisfiedBy_of_source_spec_with_or()
        {
            Assert.False(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True(goldUserSpec.Or(silverUserSpec).IsSatisfiedBy(goldUser));

        }

        [Test]
        public virtual void Or_specification_should_union_result_of_SatisfyingElementsFrom_of_source_spec()
        {
            var users = new[] { simpleUser, silverUser, goldUser };

            Assert.AreEqual(2, goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Where(u => u == goldUser).Any());
            Assert.True(goldUserSpec.Or(silverUserSpec).SatisfyingElementsFrom(users).Where(u => u == silverUser).Any());

        }


        [Test]
        public virtual void Or_specification_operator_overload_should_work()
        {
            Assert.False((goldUserSpec | silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False((goldUserSpec | silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.True((goldUserSpec | silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True((goldUserSpec | silverUserSpec).IsSatisfiedBy(goldUser));

        }


        [Test]
        public virtual void And_specification_should_compare_result_of_IsSatisfiedBy_of_source_spec_with_and()
        {
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.False(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True(goldUserSpec.And(silverUserSpec).IsSatisfiedBy(goldUser));

        }


        [Test]
        public virtual void And_specification_should_intersect_result_of_SatisfyingElementsFrom_of_source_spec()
        {
            var users = new[] { simpleUser, silverUser, goldUser };

            Assert.AreEqual(1, goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Count());
            Assert.True(goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Where(u => u == goldUser).Any());
            Assert.False(goldUserSpec.And(silverUserSpec).SatisfyingElementsFrom(users).Where(u => u == silverUser).Any());

        }


        [Test]
        public virtual void And_specification_operator_overload_should_work()
        {
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(notUser));
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(simpleUser));
            Assert.False((goldUserSpec & silverUserSpec).IsSatisfiedBy(silverUser));
            Assert.True((goldUserSpec & silverUserSpec).IsSatisfiedBy(goldUser));

        }

        [Test]
        public virtual void Complex_specification_operator_overload_combination_should_work_for_IsSatisfiedBy()
        {
            Assert.True(((goldUserSpec & silverUserSpec) | (!silverUserSpec)).IsSatisfiedBy(simpleUser));
            Assert.True(((goldUserSpec & silverUserSpec) | (!goldUserSpec)).IsSatisfiedBy(notUser));
        }

        [Test]
        public virtual void Complex_specification_operator_overload_combination_should_work_for_SatisfyingElementsFrom()
        {
            var users = new[] { simpleUser, silverUser, goldUser };
            Assert.AreEqual(3, ((goldUserSpec & silverUserSpec) | (!goldUserSpec)).SatisfyingElementsFrom(users).Count());
        }


        class User : IDomainObject
        {
            public decimal Balance { get; set; }
        }

        class GoldUserSpecification: Specification<User>
        {
            public override IEnumerable<User> SatisfyingElementsFrom(IEnumerable<User> candidates)
            {
                return candidates.Where(u => u.Balance > 100);
            }
        }


        class SilverUserSpecification : Specification<User>
        {
            public override IEnumerable<User> SatisfyingElementsFrom(IEnumerable<User> candidates)
            {
                return candidates.Where(u => u.Balance >= 100);
            }
        }

    }
 // ReSharper restore InconsistentNaming

}