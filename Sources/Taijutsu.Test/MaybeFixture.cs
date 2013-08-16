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

using System;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MaybeFixture
    {
        private readonly Maybe<Customer> emptyMaybe = Maybe<Customer>.Empty;

        private readonly Customer customer = new Customer();

        private Maybe<Customer> maybe;

        [SetUp]
        public void OnSetUp()
        {
            maybe = customer;
        }

        [Test]
        public virtual void EmptyHasValueShouldReturnFalse()
        {
            emptyMaybe.HasValue.Should().Be.False();
            Assert.IsFalse(emptyMaybe);
        }

        [Test]
        public virtual void EmptyToStringShouldReturnStringForEmptyMaybe()
        {
            emptyMaybe.ToString().Should().Not.Be.NullOrEmpty();
            emptyMaybe.ToString().Should().Contain(typeof(Customer).ToString());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public virtual void EmptyValueShouldNotBeAccessible()
        {
            // ReSharper disable once UnusedVariable
            var val = emptyMaybe.Value;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public virtual void EmptyValueShouldNotBeAccessibleThroughCastOperator()
        {
            // ReSharper disable UnusedVariable
            var val = (Customer)emptyMaybe;

            // ReSharper restore UnusedVariable
        }

        [Test]
        public virtual void HasValueShouldReturnTrue()
        {
            maybe.HasValue.Should().Be.True();
            Assert.IsTrue(maybe);
        }

        [Test]
        public virtual void ToStringShouldReturnStringForPayload()
        {
            maybe.ToString().Should().Be.EqualTo(customer.ToString());
        }

        [Test]
        public virtual void ValueShouldBeAccessible()
        {
            maybe.Value.Should().Be.SameInstanceAs(customer);
            ((Customer)maybe).Should().Be.SameInstanceAs(customer);
        }

        [Test]
        public virtual void MaybeExMethodShouldReturnNotEmptyMaybe()
        {
            customer.Maybe().HasValue.Should().Be.True();
        }
    }
}