﻿// Copyright 2009-2013 Nikita Govorov
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Management.Instrumentation;

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DataContextFixture : TestFixture
    {
        private string source;

        private IDataSession session;

        [SetUp]
        public void OnSetUp()
        {
            source = Guid.NewGuid().ToString();
            session = Substitute.For<IDataSession>();
            InternalEnvironment.RegisterDataSource(new DataSource(source, il => session));
        }

        [TearDown]
        public void OnTearDown()
        {
            session.ClearReceivedCalls();
            InternalEnvironment.UnregisterDataSource(source);
            InternalEnvironment.CheckDataContextSupervisorForRelease();
        }

        [Test]
        public virtual void ShouldCallRealCompleteOnlyOnce()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            using (var context = new DataContext(config, sessionBuilder, policy))
            {
                Awaken(context);
                context.Complete();
                context.Complete();
            }

            session.Received(1).Complete();
        }

        [Test]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed. Acceptable for tests.")]
        public virtual void ShouldCallRealDisposeOnlyOnce()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);

            Awaken(context);

            context.Dispose();
            context.Dispose();

            session.Received(1).Dispose();
        }

        [Test]
        public virtual void ShouldNotCallRealCompleteIfSessionHasNotBeenUsed()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            using (var context = new DataContext(config, sessionBuilder, policy))
            {
                context.Complete();
            }

            session.DidNotReceive().Complete();
        }

        [Test]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed. Acceptable for tests.")]
        public virtual void ShouldNotCallRealDisposeIfSessionHasNotBeenUsed()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);

            context.Dispose();
            context.Dispose();

            session.DidNotReceive().Complete();
        }

        [Test]
        public virtual void ShouldThrowExceptionIfCompleteCalledAfterDispose()
        {
            Assert.That(
                () =>
                {
                    var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
                    var sessionBuilder = new Lazy<IDataSession>(() => session, false);
                    var policy = new ImmediateTerminationPolicy();

                    var context = new DataContext(config, sessionBuilder, policy);
                    Awaken(context);
                    context.Dispose();
                    context.Complete();
                }, 
                Throws.Exception.With.Message.EqualTo("Data context has already been disposed(with success - 'False'), so it is not usable anymore."));

            session.Received(1).Dispose();
            session.Received(0).Complete();
        }

        [Test]
        public virtual void ShouldRaiseFinishedEventDuringDispose()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            bool? success = null;

            var context = new DataContext(config, sessionBuilder, policy);

            Awaken(context);

            context.Finished += (sender, e) =>
            {
                success = e.Completed;
                session.Received(0).Dispose();
            };

            context.Dispose();

            session.Received(1).Dispose();
            success.Should().Not.Be(null);
            success.Should().Be.EqualTo(false);

            session.ClearReceivedCalls();

            success = null;

            context = new DataContext(config, sessionBuilder, policy);

            Awaken(context);

            context.Finished += (sender, e) =>
            {
                success = e.Completed;
                session.Received(0).Dispose();
                session.Received(1).Complete();
            };
            context.Complete();
            context.Dispose();
            success.Should().Not.Be(null);
            success.Should().Be.EqualTo(true);
            session.Received(1).Dispose();

            session.ClearReceivedCalls();
        }

        [Test]
        public virtual void ShouldRaiseFinishedEventDuringDecoratorDispose()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);
            var contextDecorator = new DataContextSupervisor.DataContextDecorator(context, new List<DataContextSupervisor.DataContextDecorator>());

            Awaken(contextDecorator);

            EventHandler<FinishedEventArgs> eventHandler = (sender, e) => { throw new InstanceNotFoundException(); };
            ((IDataContext)contextDecorator).Finished += eventHandler;

            try
            {
                contextDecorator.Dispose();
            }
            catch (InstanceNotFoundException)
            {
            }

            session.Received(1).Dispose();

            session.ClearReceivedCalls();

            context = new DataContext(config, sessionBuilder, policy);
            contextDecorator = new DataContextSupervisor.DataContextDecorator(context, new List<DataContextSupervisor.DataContextDecorator>());

            Awaken(contextDecorator);

            eventHandler = (s, e) => { throw new InstanceNotFoundException(); };
            ((IDataContext)contextDecorator).Finished += eventHandler;
            ((IDataContext)contextDecorator).Finished -= eventHandler;
            contextDecorator.Dispose();
            session.Received(1).Dispose();
        }

        [Test]
        public virtual void ShouldReplaceFinishedEventInSubordinate()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var called = false;

            var context = new DataContext(config, sessionBuilder, policy);

            var subordinate = new DataContext.Subordinate(context);

            Awaken(subordinate);

            ((IDataContext)subordinate).Finished += (sender, e) => { called = true; };

            context.Dispose();

            called.Should().Be.True();

            session.Received(1).Dispose();

            session.ClearReceivedCalls();

            called = false;
            context = new DataContext(config, sessionBuilder, policy);
            subordinate = new DataContext.Subordinate(context);

            Awaken(subordinate);

            EventHandler<FinishedEventArgs> eventHandler = (sender, e) => { called = true; };
            ((IDataContext)subordinate).Finished += eventHandler;
            ((IDataContext)subordinate).Finished -= eventHandler;
            context.Dispose();
            session.Received(1).Dispose();
            called.Should().Be.False();
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Unit of work can not be successfully completed, because not all subordinates are completed.")]
        public virtual void ShouldThrowExceptionOnCompleteIfOneOfTheChildrenHasNotBeenCompleted()
        {
            using (var uowla = new UnitOfWork(source))
            {
                using (var uowlai2 = new UnitOfWork(source))
                {
                    using (new UnitOfWork(source))
                    {
                    }

                    uowlai2.Complete();
                }

                uowla.Complete();
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Data context has already been disposed(with success - 'False'), so it is not usable anymore.")]
        public virtual void ShouldThrowExceptionOnSessionCallIfDisposeHasAlreadyBeenCalled()
        {
            var config = new UnitOfWorkConfig(string.Empty, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IDataSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);
            context.Dispose();
            Awaken(context);
        }
    }
}