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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Domain;
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
            private LimitReached limitReached100;
            private LimitReached limitReached200;
            private LimitReached limitReached250;
            private LimitReached limitReached300;
            private UserBlocked userBlocked1;
            private UserBlocked userBlocked2;
            private UserBlocked userBlocked3;
            private Customer customerNikita;
            private Customer customerEgor;
            private Product product;
            private InternetOrder internetOrder1000;
            private Order order500;
            private int callCounter;
            private Order order700;
            private Order order600;
            private InternetOrder internetOrder2000;
            private InternetOrder internetOrder3000;

            [TearDown]
            protected void OnTearDown()
            {
            }

            [SetUp]
            protected void OnSetUp()
            {
                limitReached100 = new LimitReached(100);
                limitReached200 = new LimitReached(200);
                limitReached250 = new LimitReached(250);
                limitReached300 = new LimitReached(300);

                userBlocked1 = new UserBlocked(1);
                userBlocked2 = new UserBlocked(2);
                userBlocked3 = new UserBlocked(3);

                customerNikita = new Customer(new FullName("Nikita", "Govorov"));
                customerEgor = new Customer(new FullName("Egor", "Govorov"));

                order500 = new Order {Total = 500};
                order600 = new Order {Total = 600};
                order700 = new Order {Total = 700};

                internetOrder1000 = new InternetOrder {Total = 1000};
                internetOrder2000 = new InternetOrder {Total = 2000};
                internetOrder3000 = new InternetOrder {Total = 3000};

                product = new Product();

                callCounter = 0;
            }

            [Test]
            public virtual void SubscribeLambda()
            {
                EventAggregator.OnStream.OfEvents.AddressedTo<Order>()
                               .Subscribe(ev =>
                                   {
                                       callCounter++;
                                       ev.Recipient.Total.Should().Be.GreaterThan(0);
                                   });

                EventAggregator.Publish(new ExternalEvent<Order>(order600)); //+1
                EventAggregator.Publish(new ExternalEvent<InternetOrder>(internetOrder3000));
                EventAggregator.Publish(new ExternalEvent<Customer>(customerNikita));
                EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder2000, limitReached200)); 
                EventAggregator.Publish(new ExternalEvent<Product, LimitReached>(product, limitReached300));
                EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order700, limitReached250)); //+1


                callCounter.Should().Be(2);
            }


            [Test]
            public virtual void Perf()
            {

                var random = new Random();

                var handlers = new Dictionary<string, string>();

                for (var i = 0; i < 1000; i++)
                {
                    handlers.Add(i.ToString(), i.ToString());
                }

                var types = new List<string>();

                for (int i = 0; i < 20; i++)
                {
                    types.Add(random.Next(0, 9999).ToString());
                }

                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    for (int j = 0; j < types.Count; j++)
                    {
                        string result = "";
                        handlers.TryGetValue(types[j], out result);
                    }
                }
                sw.Stop();
                Trace.TraceInformation("Elapsed: " + sw.Elapsed.ToString());

            }

            [Test]
            public virtual void Test()
            {
                
                var gt = typeof(IExternalEvent<InternetOrder>);

                gt.IsGenericType.Should().Be.True();
                gt.GenericTypeArguments.Length.Should().Be(1);
                gt.GenericTypeArguments[0].Should().Be(typeof(InternetOrder));

                var gtd = gt.GetGenericTypeDefinition();
                
                var internetOrderType = gt.GenericTypeArguments[0];
                
                var internetOrderGenericArgument = gtd.GetGenericArguments()[0];

                var constraints = internetOrderGenericArgument.GetGenericParameterConstraints();

                var types = PotentialTypes(internetOrderType, constraints);

                var generics = new List<Type>();

                foreach (var type in types)
                {
                    generics.Add(gtd.MakeGenericType(new[] { type }));
                }

                var test = PotentialSubscriberTypes(typeof (ExternalEvent<Order, LimitReached>));

                var res = generics.Union(test).Distinct();

                generics.Should().Have.Count.EqualTo(5);
            }

            protected virtual IEnumerable<Type> EventTypeHierarchy2(Type type, Type[] constrints)
            {
                if (constrints.Any(i=>i.IsAssignableFrom(type)))
                {
                    yield return type;
                    foreach (var subtype in EventTypeHierarchy2(type.BaseType, constrints))
                    {
                        yield return subtype;
                    }
                }
            }

            protected virtual IEnumerable<Type> PotentialTypes(Type type, Type[] constrints)
            {
                IEnumerable<Type> targetsForType;

                targetsForType =
                    type.GetInterfaces()
                        .Where(i => constrints.Any(c => c.IsAssignableFrom(i)))
                        .Union(EventTypeHierarchy2(type, constrints).Reverse()).Distinct()
                        .ToArray();

                return targetsForType as Type[];
            }

            protected virtual IEnumerable<Type> PotentialSubscriberTypes(Type type)
            {
                IEnumerable<Type> targetsForType;

                targetsForType =
                    type.GetInterfaces()
                        .Where(i => typeof(IEvent).IsAssignableFrom(i))
                        .Union(EventTypeHierarchy(type).Reverse())
                        .ToArray();

                return targetsForType as Type[] ?? targetsForType.ToArray();
            }

            protected virtual IEnumerable<Type> EventTypeHierarchy(Type type)
            {
                if (typeof(IEvent).IsAssignableFrom(type))
                {
                    yield return type;
                    foreach (var subtype in EventTypeHierarchy(type.BaseType))
                    {
                        yield return subtype;
                    }
                }
            }


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
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact1)); //+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact1)); //+1
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
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2)); //+1
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
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact3)); //+1
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact4));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer2, fact4));
                    EventAggregator.Publish(new ExternalEvent<Customer, UserBlocked>(customer2, fact5));
                    EventAggregator.Publish(new ExternalEvent<Customer, LimitReached>(customer1, fact2));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact3)); //+1
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact4));
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact3)); //+1
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
                            //ev.Fact.Amount.Should().Be.GreaterThan(0);
                        })
                    .AsDisposable())
                {
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact1));
                    EventAggregator.Publish(new ExternalEvent<Order, UserBlocked>(order1, fact5));
                    EventAggregator.Publish(new ExternalEvent<InternetOrder, LimitReached>(internetOrder1, fact3)); //+1
                    EventAggregator.Publish(new ExternalEvent<Order, LimitReached>(order1, fact2)); //+1
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