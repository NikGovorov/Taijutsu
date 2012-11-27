#region License

//  Copyright 2009-2013 Nikita Govorov
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

using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Domain.Event;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    //[TestFixture]
    public class AddressedToSyntaxFixture
    {
        // ReSharper disable AccessToModifiedClosure
        
        [TestFixture]
        public class Subscribing
        {

            [Test]
            public virtual void Temp()
            {

                var fact1 = new LimitReached(100);
                var fact2 = new LimitReached(200);
                var fact3 = new LimitReached(250);
                var fact4 = new LimitReached(300);

                var fact5 = new UserBlocked();

                var customer1 = new Customer(new FullName("Nikita", "Govorov"));
                var customer2 = new Customer(new FullName("Egor", "Govorov"));

                var product1 = new Product();

                var order1 = new Order();

                var internetOrder1 = new InternetOrder();

                var callCounter = 0;

                using (EventAggregator
                    .OnStream.OfEvents
                    .AddressedTo<Customer>()
                    .DueTo<LimitReached>()
                    .Where(ev => ev.Fact.Amount > 50 && ev.Fact.Amount < 200)
                    .Subscribe(ev =>
                    {
                        callCounter++;
                        ev.Recipient.Name.FirstName.Should().Be("Nikita");
                        ev.Fact.Amount.Should().Be(100);
                    })
                    .AsDisposable())
                {
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact1));//+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact1));//+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                }

                callCounter.Should().Be(2);

                callCounter = 0;

                using (EventAggregator
                    .OnStream.OfEvents
                    .AddressedTo<Customer>()
                    .Where(ev => ev.Recipient.Name.FirstName == "Nikita")
                    .Select(ev => ev.Recipient)
                    .Subscribe(customer =>
                    {
                        callCounter++;
                        customer.Name.FirstName.Should().Be("Nikita");
                    })
                    .AsDisposable())
                {
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));//+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact1));
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                }

                callCounter.Should().Be(1);

                callCounter = 0;


                using (EventAggregator
                    .OnStream.OfEvents
                    .AddressedTo<Customer>()
                    .Or.AddressedTo<Order>()
                    .DueTo<LimitReached>()
                    .Where(ev => ev.Fact.Amount > 100)
                    .Where(ev => ev.Fact.Amount < 300)
                    .Select(ev => ev.Fact.Amount)
                    .Where(amount => amount > 200)
                    .Subscribe(amount =>
                    {
                        callCounter++;
                        amount.Should().Be(250);
                    })
                    .AsDisposable())
                {
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact1));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact1));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact3));//+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact4));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact4));
                    EventAggregator.Publish(new ExternalEvent<Customer, UserBlocked>(customer2, fact5));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact3));//+1
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact4));
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact3));//+1
                    EventAggregator.Publish(new ExternalEvent<Product, LimitReached>(product1, fact3));
                }

                callCounter.Should().Be(3);

                callCounter = 0;

                using (EventAggregator
                    .OnStream.OfEvents
                    .AddressedTo<Order>()
                    .DueTo<LimitReached>()
                    .Subscribe(ev =>
                    {
                        callCounter++;
                        ev.Fact.Amount.Should().Be.GreaterThan(0);
                    })
                    .AsDisposable())
                {
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                    EventAggregator.Publish(new ExternalEvent<Order, UserBlocked>(order1, fact5));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact3));//+1
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact2));//+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));
                }

                callCounter.Should().Be(2);
            }
             
        }

        public class Publishing
        {
             
        }
        // ReSharper restore AccessToModifiedClosure
    }
}