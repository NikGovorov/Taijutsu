/*// Copyright 2009-2013 Nikita Govorov
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

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Domain;
using Taijutsu.Event;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EntityFixture
    {
        [Test]
        public virtual void ShouldBeComparedByTypeAndId()
        {
            var sharedId = SeqGuid.NewGuid();

            var customer = new Customer(sharedId, new FullName("Nik", "Gov"));
            var customer2 = new Customer(SeqGuid.NewGuid(), new FullName("Nik", "Gov"));
            var customer3 = new Customer(sharedId, new FullName("Nik", "Gov"));

            var order = new Order(sharedId);
            var internetOrder = new InternetOrder(sharedId);

            Assert.IsTrue(customer != customer2);
            Assert.IsFalse(customer.Equals(customer2));
            Assert.IsFalse(customer.Equals((object)customer2));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Assert.IsTrue(customer != null);
            Assert.IsFalse(customer.Equals(null));

            Assert.IsTrue(customer == customer3);
            Assert.IsTrue(customer.Equals(customer3));
            Assert.IsTrue(customer.Equals((object)customer3));

            Assert.IsTrue(customer != order);
            Assert.IsFalse(customer.Equals(order));

            // ReSharper disable SuspiciousTypeConversion.Global
            Assert.IsFalse(customer.Equals((object)order));

            // ReSharper restore SuspiciousTypeConversion.Global
            Assert.IsTrue(order != internetOrder);
            Assert.IsFalse(order.Equals(internetOrder));
            Assert.IsFalse(order.Equals((object)internetOrder));

            var product1 = new Product();
            var product2 = new Product();
            var product3 = new Product(3);
            var product4 = new Product(4);

            Assert.IsTrue(product1 != product2);
            Assert.IsTrue(product3 != product2);
            Assert.IsTrue(product4 != product3);
        }

        [Test]
        public virtual void GetHashCodeShouldReturnConstantDuringEntityLifePeriod()
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

        [Test]
        public virtual void ToStringShouldReturnId()
        {
            var sharedkey = SeqGuid.NewGuid();
            var internetOrder = new InternetOrder(sharedkey);
            internetOrder.ToString().Should().Be(sharedkey.ToString());
        }

        [Test]
        public virtual void ShouldBeAbleToPublishEvents()
        {
            Order expectedOrder = null;

            using (Events.OnStreamOf<OrderCreated>().Subscribe(ev => expectedOrder = ev.Initiator).AsDisposable())
            {
                var internetOrder = new InternetOrder(new Customer());

                expectedOrder.Should().Not.Be.Null();
                expectedOrder.Should().Be.SameInstanceAs(internetOrder);
            }
        }

        [Test]
        public virtual void ShouldBeAbleToSubscribeOnEvents()
        {
            var customer = new Customer();
            customer.NotifiedAboutOrder.Should().Be.False();
#pragma warning disable 168
            // ReSharper disable once UnusedVariable
            var internetOrder = new InternetOrder(customer);
#pragma warning restore 168
            customer.NotifiedAboutOrder.Should().Be.True();
        }

        private class TestEntity : Entity<Guid>
        {
            public TestEntity()
            {
            }

            public TestEntity(Guid id)
            {
                this.id = id;
            }

            public void SetKey(Guid newId)
            {
                id = newId;
            }
        }
    }
}*/