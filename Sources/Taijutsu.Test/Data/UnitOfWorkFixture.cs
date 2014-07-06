// Copyright 2009-2014 Nikita Govorov
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
using System.Diagnostics.CodeAnalysis;

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class UnitOfWorkFixture : TestFixture
    {
        private const string InteractAfterComplete = "Unit of work has already been completed(with success - '{0}'), so it is not usable for write anymore.";

        private const string InteractAfterDispose = "Unit of work has already been disposed(with success - '{0}'), so it is not usable anymore.";

        private IDataSession session;

        private string source;

        [SetUp]
        public void OnSetUp()
        {
            source = Guid.NewGuid().ToString();
            session = Substitute.For<IDataSession>();
            DataEnvironment.RegisterDataSource(new DataSource(source, il => session));
        }

        [TearDown]
        public void OnTearDown()
        {
            DataEnvironment.UnregisterDataSource(source);
            DataEnvironment.CheckDataContextSupervisorForRelease();
            session.ClearReceivedCalls();
        }

        [Test]
        public virtual void ShouldProvideDifferentWaysOfComplete()
        {
            using (var uow = new UnitOfWork(source))
            {
                Awaken(uow);

                uow.Complete(
                    () =>
                    {
                        session.Received(0).Complete();
                        return "test";
                    }).Should().Be("test");

                uow.Complete(
                    uow2 =>
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
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed. Acceptable for tests.")]
        public virtual void ShouldCallRealDisposeOnlyOnce()
        {
            var uow = new UnitOfWork(source);

            Awaken(uow);

            AssertThatContextCountEqualTo(1);
            ((IDisposable)uow).Dispose();

            AssertThatSupervisorDestroyed();
            AssertThatContextCountEqualTo(0);

            ((IDisposable)uow).Dispose();

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
        public virtual void ShouldDelegateCreateMethodToDataSession()
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
                customer.AsCreatedIn(uow);
            }

            session.Received(1).MarkAsCreated(customer as IAggregateRoot);
        }

        [Test]
        public virtual void ShouldDelegateDeleteMethodToDataSession()
        {
            var options = new { };

            var customer = new Customer(SeqGuid.NewGuid(), new FullName("Nikita", "Govorov"));

            using (var uow = new UnitOfWork(source))
            {
                uow.MarkAsDeleted(customer, options);
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
        public virtual void ShouldThrowExceptionIfCreateCalledAfterComplete()
        {
            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    uow.Complete();
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsCreated(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(
                () =>
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        uow.Complete();
                        uow.MarkAsCreated(() => new Customer());
                    }
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(
                () =>
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        uow.Complete();
                        uow.MarkAsCreated(new Customer());
                    }
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(
                () =>
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        uow.Complete();
                        new Customer().AsCreatedIn(uow);
                    }
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfCreateCalledAfterDispose()
        {
            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsCreated(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsCreated(() => new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsCreated(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    new Customer().AsCreatedIn(uow);
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfDeleteCalledAfterComplete()
        {
            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    uow.Complete();
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsDeleted(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(
                () =>
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        uow.Complete();
                        uow.MarkAsDeleted(new Customer());
                    }
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));

            Assert.That(
                () =>
                {
                    using (var uow = new UnitOfWork(source))
                    {
                        uow.Complete();
                        new Customer().AsDeletedIn(uow);
                    }
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, true)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfDeleteCalledAfterDispose()
        {
            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsDeleted(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    uow.MarkAsDeleted(new Customer());
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));

            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    ((IDisposable)uow).Dispose();
                    new Customer().AsDeletedIn(uow);
                }, 
                Throws.Exception.With.Message.EqualTo(string.Format(InteractAfterComplete, false)));
        }

        [Test]
        public virtual void ShouldThrowExceptionIfCompleteCalledAfterDispose()
        {
            Assert.That(
                () =>
                {
                    var uow = new UnitOfWork(source);
                    Awaken(uow);
                    ((IDisposable)uow).Dispose();
                    uow.Complete();
                }, 
                Throws.Exception.With.Message.EqualTo("Unit of work has already been disposed(with success - 'False'), so it is not usable anymore."));

            session.Received(1).Dispose();
            session.Received(0).Complete();
        }
    }
}