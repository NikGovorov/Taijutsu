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
using System.Collections.Generic;
using NUnit.Framework;
using Taijutsu.Domain;
using Taijutsu.Specs.Domain.Model;

namespace Taijutsu.Specs.Domain
{
    // ReSharper disable InconsistentNaming
    public class Entity_Specs
    {
        [Test]
        public virtual void Equality_comparison_should_use_build_identity_method_which_compares_type_and_key()
        {
            var sharedkey = SeqGuid.NewGuid();
            
            var customer = new Customer(sharedkey, new FullName("Nik", "Gov"));
            var customer2 = new Customer(SeqGuid.NewGuid(), new FullName("Nik", "Gov"));
            var customer3 = new Customer(sharedkey, new FullName("Nik", "Gov"));

            var order = new Order(sharedkey);
            var internetOrder = new InternetOrder(sharedkey);

            Assert.IsTrue(customer != customer2);
            Assert.IsFalse(customer.Equals(customer2));
            Assert.IsFalse(customer.Equals((object)customer2));
            Assert.IsTrue(customer != null);

            Assert.IsTrue(customer == customer3);
            Assert.IsTrue(customer.Equals(customer3));
            Assert.IsTrue(customer.Equals((object)customer3));

            Assert.IsTrue(customer != order);
            Assert.IsFalse(customer.Equals(order));
            Assert.IsFalse((customer.Equals((object)order)));

            Assert.IsTrue(order != internetOrder);
            Assert.IsFalse(order.Equals(internetOrder));
            Assert.IsFalse((order.Equals((object)internetOrder)));

            var product1 = new Product();
            var product2 = new Product();
            var product3 = new Product(3);
            var product4 = new Product(4);

            Assert.IsTrue(product1 != product2);
            Assert.IsTrue(product3 != product2);
            Assert.IsTrue(product4 != product3);

        }

        [Test]
        public virtual void Get_has_code_should_be_constant_during_entity_life_period()
        {
            var key = Guid.NewGuid();
            var set = new HashSet<TestEntity>();
            var entity = new TestEntity();
            var entity2 = new TestEntity(key);
            Assert.IsTrue(set.Add(entity));
            Assert.IsTrue(set.Add(entity2));
            var hashCode = entity.GetHashCode();
            Assert.IsFalse(entity == entity2);
            entity.SetKey(key);
            Assert.AreEqual(hashCode, entity.GetHashCode());
            Assert.AreNotEqual(hashCode, entity2.GetHashCode());
            Assert.IsFalse(set.Add(entity));
            Assert.IsTrue(entity == entity2);
        }


        class TestEntity: Entity<Guid>
        {
            public TestEntity()
            {
            }

            public TestEntity(Guid key)
            {
                entityKey = key;
            }

            public virtual void SetKey(Guid guid)
             {
                 entityKey = guid;
             }
        }

    }
    // ReSharper restore InconsistentNaming
}