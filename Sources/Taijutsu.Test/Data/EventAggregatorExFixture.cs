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
using System.Transactions;

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Event;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EventAggregatorExFixture
    {
        [TestFixture]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class HandledSafelyMethod
        {
            private string source;

            private IOrmSession session;

            [SetUp]
            public void OnSetUp()
            {
                source = Guid.NewGuid().ToString();
                session = Substitute.For<IOrmSession>();
                InternalEnvironment.RegisterDataSource(new DataSource(source, il => session));
            }

            [TearDown]
            public void OnTearDown()
            {
                InternalEnvironment.UnregisterDataSource(source);
                InternalEnvironment.UnregisterOperationScope();
                session.ClearReceivedCalls();
            }

            [Test]
            public virtual void ShouldNotExecuteHandlerOutsideOfUnitOfWork()
            {
                var callCounter = 0;

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter++).AsDisposable())
                {
                    EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                }

                callCounter.Should().Be(0);
            }

            [Test]
            public virtual void ShouldBufferEventsAndExecuteHandlersAfterUnitOfWorkComplete()
            {
                int[] callCounter = { 0 };

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter[0]++).AsDisposable())
                {
                    using (new UnitOfWork(source))
                    {
                        EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                        callCounter[0].Should().Be(0);
                    }

                    callCounter[0].Should().Be(0);
                }

                callCounter[0] = 0;

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter[0]++).AsDisposable())
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                        callCounter[0].Should().Be(0);
                        uow.Complete();
                        callCounter[0].Should().Be(0);
                    }

                    callCounter[0].Should().Be(1);
                }
            }

            [Test]
            public virtual void ShouldBufferEventsAndExecuteHandlersAfterTransactionScopeComplete()
            {
                int[] callCounter = { 0 };

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter[0]++).AsDisposable())
                {
                    using (new TransactionScope())
                    {
                        EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                        callCounter[0].Should().Be(0);
                    }

                    callCounter[0].Should().Be(0);
                }

                callCounter[0] = 0;

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter[0]++).AsDisposable())
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                        callCounter[0].Should().Be(0);
                        transactionScope.Complete();
                        callCounter[0].Should().Be(0);
                    }

                    callCounter[0].Should().Be(1);
                }

                callCounter[0] = 0;

                using (EventAggregator.OnStreamOf<OrderCreated>().HandledSafely().Subscribe(ev => callCounter[0]++).AsDisposable())
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                        callCounter[0].Should().Be(0);
                        using (var uow = new UnitOfWork(source))
                        {
                            EventAggregator.Publish(new OrderCreated(new Order(), new Customer()));
                            callCounter[0].Should().Be(0);
                            uow.Complete();
                            callCounter[0].Should().Be(0);
                        }

                        callCounter[0].Should().Be(0);

                        callCounter[0].Should().Be(0);
                        transactionScope.Complete();
                        callCounter[0].Should().Be(0);
                    }

                    callCounter[0].Should().Be(2);
                }
            }
        }
    }
}