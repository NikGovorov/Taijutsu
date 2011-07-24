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
using NUnit.Framework;
using Taijutsu.Domain.Specs.Domain;

namespace Taijutsu.Domain.Specs
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class When_using_empty_maybe : ContextSpecification
    {
        private Maybe<Customer> maybeCustomer;

        protected override void When()
        {
            maybeCustomer = Maybe<Customer>.Empty;
        }

        protected override void Because()
        {
        }

        [Test]
        public virtual void has_value_should_return_false()
        {
            Assert.IsFalse(maybeCustomer.HasValue);
            Assert.IsFalse(maybeCustomer);
        }

        [Test]
        public virtual void to_string_should_return_string_for_empty_maybe()
        {
            Assert.IsNotNullOrEmpty(maybeCustomer.ToString());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public virtual void value_should_not_be_accessible()
        {
            var val = maybeCustomer.Value;
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public virtual void value_should_not_be_accessible_through_cast_operator()
        {
            Customer val = (Customer)maybeCustomer;
        }

    }


    [TestFixture]
    public class When_using_not_empty_maybe : ContextSpecification
    {
        private Maybe<Customer> maybeCustomer;
        private Customer customer;

        protected override void When()
        {
            customer = new Customer();
            maybeCustomer = new Maybe<Customer>(customer);
        }

        protected override void Because()
        {
            maybeCustomer = customer;
        }

        [Test]
        public virtual void has_value_should_return_true()
        {
            Assert.IsTrue(maybeCustomer.HasValue);
            Assert.IsTrue(maybeCustomer);
        }

        [Test]
        public virtual void to_string_should_return_string_for_customer()
        {
            Assert.AreEqual(customer.ToString(), maybeCustomer.ToString());
        }

        [Test]
        public virtual void value_should_be_accessible()
        {
            Assert.AreSame(customer, maybeCustomer.Value);
            Assert.AreSame(customer, (Customer)maybeCustomer); 
        }

    }

    // ReSharper restore InconsistentNaming


}