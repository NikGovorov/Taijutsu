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
using NSubstitute;
using NUnit.Framework;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using SharpTestsEx;
using Taijutsu.Test.Domain.Model;
using Taijutsu.Domain;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class UnitOfWorkFixture : TestFixture
    {
        private const string InteractAfterComplete = "Unit of work has already been completed(with success - '{0}'), so it is not usable for write anymore.";
        private const string InteractAfterDispose = "Unit of work has already been disposed(with success - '{0}'), so it is not usable anymore.";

        private IOrmSession session;
        private string source;

        [SetUp]
        protected void OnSetUp()
        {
            source = Guid.NewGuid().ToString();
            session = Substitute.For<IOrmSession>();
            InternalEnvironment.RegisterDataSource(new DataSource(source, il => session));
        }

        [TearDown]
        protected void OnTearDown()
        {
            InternalEnvironment.UnregisterDataSource(source);
            session.ClearReceivedCalls();
        }

        protected virtual void Awaken(UnitOfWork uow)
        {
            #pragma warning disable 168
            var nativeSession = ((IHasNativeObject) uow).NativeObject; // initialize lazy session
            #pragma warning restore 168
        }

        [Test]
        public virtual void ShouldProvideDifferentWaysOfComplete()
        {
            using (var uow = new UnitOfWork(source))
            {
                Awaken(uow);

                
                uow.Complete(() =>
                    {
                        session.Received(0).Complete();
                        return "test";

                    }).Should().Be("test");


                uow.Complete(uow2 =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        uow2.Should().Be.SameInstanceAs(uow);
                        // ReSharper restore AccessToDisposedClosure
                    return "test2";

                }).Should().Be("test2");


                uow.Complete(100).Should().Be(100);
                
                uow.Complete();
            }

            session.Received(1).Complete();
        }

        [Test]
        public virtual void ShouldCallRealDisposeOnlyOnce()
        {
            var uow = new UnitOfWork(source);

            Awaken(uow);

            AssertThatContextCountEqualTo(1);
            ((IDisposable) uow).Dispose();
            
            AssertThatSupervisorDestroyed();
            AssertThatContextCountEqualTo(0);
            
            ((IDisposable) uow).Dispose();

            session.Received(1).Dispose();
        }

        [Test]
        public virtual void ShouldCallRealCompleteOnlyOnce()
        {
            using (var uow = new UnitOfWork(source))
            {
                Awaken(uow);
                uow.Complete();
                uow.Complete();
                uow.Complete(100);
                uow.Complete(() => 100);
                uow.Complete(u => 100);
            }
            session.Received(1).Complete();
        }

        [Test]
        public virtual void ShouldDelegateCreateMethodToOrmSession()
        {
            var options = new { };

            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Nikita", "Govorov"));

            using (var uow = new UnitOfWork(source))
            {
                uow.MarkAsCreated(customer, options);
            }

            session.Received(1).MarkAsCreated(customer, options);

            session.ClearReceivedCalls();

            Func<Customer> factory = () => customer;

            using (var uow = new UnitOfWork(source))
            {
                uow.MarkAsCreated(factory, options);
            }

            session.Received(1).MarkAsCreated(factory, options);

            session.ClearReceivedCalls();

            using (var uow = new UnitOfWork(source))
            {
                uow.Mark(customer, options).AsCreated();
            }

            session.Received(1).MarkAsCreated(customer, options);
            
            session.ClearReceivedCalls();

            using (var uow = new UnitOfWork(source))
            {
                customer.AsCreatedIn(uow);
            }

            session.Received(1).MarkAsCreated(customer as IAggregateRoot);
        }

        [Test]
        public virtual void ShouldDelegateDeleteMethodToOrmSession()
        {
            var options = new {};
            
            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Nikita", "Govorov"));

            using (var uow = new UnitOfWork(source))
            {
                uow.MarkAsDeleted(customer, options);
            }

            session.Received(1).MarkAsDeleted(customer, options);

            session.ClearReceivedCalls();

            using (var uow = new UnitOfWork(source))
            {
                uow.Mark(customer, options).AsDeleted();
            }

            session.Received(1).MarkAsDeleted(customer, options);

            session.ClearReceivedCalls();

            using (var uow = new UnitOfWork(source))
            {
                customer.AsDeletedIn(uow);
            }

            session.Received(1).MarkAsDeleted(customer as IDeletableEntity);
        }

        [Test]
        public virtual void ShouldDelegateQueryMethodsToOrmSession()
        {
            var options = new {};
            
            using (var uow = new UnitOfWork(source))
            {
                uow.All<Customer>(options);
                uow.Unique<Customer>(1, options);
                uow.Query<Customer>().With<ICustomerQuery>("query");
                uow.Query<Customer>().From<ICustomerRepository>("repository");

            }

            session.Received(1).All<Customer>(options);
            session.Received(1).Unique<Customer>(1, options);
            session.Received(1).QueryWith<Customer, ICustomerQuery>("query");
            session.Received(1).QueryFrom<Customer, ICustomerRepository>("repository");
        }

        [Test]
        public virtual void ShouldNotThrowExceptionIfQueryCalledAfterComplete()
        {
            var options = new { };

            using (var uow = new UnitOfWork(source))
            {
                uow.Complete();
                uow.All<Customer>(options);
                uow.Unique<Customer>(1, options);
                uow.Query<Customer>().With<ICustomerQuery>("query");
                uow.Query<Customer>().From<ICustomerRepository>("repository");
            }

            session.Received(1).All<Customer>(options);
            session.Received(1).Unique<Customer>(1, options);
            session.Received(1).QueryWith<Customer, ICustomerQuery>("query");
            session.Received(1).QueryFrom<Customer, ICustomerRepository>("repository");
        }

        [Test]
        public virtual void ShouldThrowExceptionIfQueryCalledAfterDispose()
        {
            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                uow.Complete();
                ((IDisposable)uow).Dispose();
                uow.All<Customer>();

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterDispose, true)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.Unique<Customer>(100);

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterDispose, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                uow.Complete();
                ((IDisposable)uow).Dispose();
                uow.Query<Customer>().With<ICustomerQuery>("query");

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterDispose, true)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.Query<Customer>().From<ICustomerRepository>("repository");

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterDispose, false)));

        }

        [Test]
        public virtual void ShouldThrowExceptionIfCreateCalledAfterComplete()
        {
            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                uow.Complete();
                ((IDisposable)uow).Dispose();
                uow.MarkAsCreated(new Customer());

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(() =>
            {
                using (var uow = new UnitOfWork(source))
                {
                    uow.Complete();
                    uow.MarkAsCreated(() => new Customer());
                }
            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(() =>
            {
                using (var uow = new UnitOfWork(source))
                {
                    uow.Complete();
                    uow.Mark(new Customer()).AsCreated();
                }
            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(() =>
            {
                using (var uow = new UnitOfWork(source))
                {
                    uow.Complete();
                    new Customer().AsCreatedIn(uow);
                }
            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

        }

        [Test]
        public virtual void ShouldThrowExceptionIfCreateCalledAfterDispose()
        {
            Assert.That(() =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsCreated(new Customer());

                }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.MarkAsCreated(() => new Customer());

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.Mark(new Customer()).AsCreated();

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                new Customer().AsCreatedIn(uow);

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfDeleteCalledAfterComplete()
        {
            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                uow.Complete();
                ((IDisposable)uow).Dispose();
                uow.MarkAsDeleted(new Customer());

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));


            Assert.That(() =>
            {
                using (var uow = new UnitOfWork(source))
                {
                    uow.Complete();
                    uow.Mark(new Customer()).AsDeleted();
                }
            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(() =>
            {
                using (var uow = new UnitOfWork(source))
                {
                    uow.Complete();
                    new Customer().AsDeletedIn(uow);
                }
            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

        }

        [Test]
        public virtual void ShouldThrowExceptionIfDeleteCalledAfterDispose()
        {
            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.MarkAsDeleted(new Customer());

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                uow.Mark(new Customer()).AsDeleted();

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(() =>
            {
                var uow = new UnitOfWork(source);
                ((IDisposable)uow).Dispose();
                new Customer().AsDeletedIn(uow);

            }, Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfCompleteCalledAfterDispose()
        {
            Assert.That(() =>
                {
                    var uow = new UnitOfWork(source);
                    Awaken(uow);
                    ((IDisposable)uow).Dispose();
                    uow.Complete();

                }, Throws.Exception.With.Message.EqualTo("Unit of work has already been completed without success."));
            
            session.Received(1).Dispose();
            session.Received(0).Complete();
        }
    }
}