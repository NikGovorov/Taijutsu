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
using SharpTestsEx;
using Taijutsu.Specs.Domain.Model;

namespace Taijutsu.Specs.Domain
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class When_using_empty_maybe : ContextSpecification
    {
        private Maybe<Customer> maybeCustomer;

        protected override void Given()
        {

        }
        
        protected override void When()
        {
            maybeCustomer = Maybe<Customer>.Empty;
        }


        [Test]
        public virtual void then_has_value_should_return_false()
        {
            maybeCustomer.HasValue.Should().Be.False();
            Assert.IsFalse(maybeCustomer);
        }

        [Test]
        public virtual void then_to_string_should_return_string_for_empty_maybe()
        {
            maybeCustomer.ToString().Should().Not.Be.NullOrEmpty();
            maybeCustomer.ToString().Should().Contain(typeof (Customer).ToString());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public virtual void then_value_should_be_not_accessible()
        {
            // ReSharper disable UnusedVariable
            var val = maybeCustomer.Value;
            // ReSharper restore UnusedVariable
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public virtual void then_value_should_not_be_accessible_through_cast_operator()
        {
            // ReSharper disable UnusedVariable
            var val = (Customer)maybeCustomer;
            // ReSharper restore UnusedVariable
        }

        [Test]
        public virtual void then_handle_should_be_invoked()
        {
            var invoked = false;
            maybeCustomer.Handle(() => invoked = true);
            invoked.Should().Be.True();
        }

        [Test]
        public virtual void then_apply_should_not_be_invoked()
        {
            var invoked = false;
            maybeCustomer.Apply((_) => invoked = true);
            invoked.Should().Be.False();
        }

    }


    [TestFixture]
    public class When_using_not_empty_maybe : ContextSpecification
    {
        private Maybe<Customer> maybeCustomer;
        private Customer customer;


        protected override void Given()
        {
            customer = new Customer(new FullName("Test", "Test"));
        }
        
        protected override void When()
        {
            maybeCustomer = customer;
        }

        [Test]
        public virtual void then_has_value_should_return_true()
        {
            maybeCustomer.HasValue.Should().Be.True();
            Assert.IsTrue(maybeCustomer);
        }

        [Test]
        public virtual void then_to_string_should_return_string_for_customer()
        {
            maybeCustomer.ToString().Should().Be.EqualTo(customer.ToString());
        }

        [Test]
        public virtual void then_value_should_be_accessible()
        {
            maybeCustomer.Value.Should().Be.SameInstanceAs(customer);
            ((Customer)maybeCustomer).Should().Be.SameInstanceAs(customer);
        }


        [Test]
        public virtual void then_handle_should_not_be_invoked()
        {
            var invoked = false;
            maybeCustomer.Handle(() => invoked = true);
            invoked.Should().Be.False();
        }

        [Test]
        public virtual void then_apply_should_be_invoked()
        {
            var invoked = false;
            maybeCustomer.Apply((_) => invoked = true);
            invoked.Should().Be.True();
        }


        [Test]
        public virtual void then_maybe_ex_method_should_return_not_empty_maybe()
        {
            customer.Maybe().HasValue.Should().Be.True();
        }
    }

    // ReSharper restore InconsistentNaming


}