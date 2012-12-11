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
using System.Data;
using NSubstitute;
using NUnit.Framework;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using SharpTestsEx;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class DataContextFixture : TestFixture
    {
        private string source;
        private IOrmSession session;

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
            session.ClearReceivedCalls();
            InternalEnvironment.UnregisterDataSource(source);
        }

        [Test]
        public virtual void ShouldCallRealCompleteOnlyOnce()
        {
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
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
        public virtual void ShouldCallRealDisposeOnlyOnce()
        {
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
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
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            using (var context = new DataContext(config, sessionBuilder, policy))
            {
                context.Complete();
            }
            session.DidNotReceive().Complete();
        }

        [Test]
        public virtual void ShouldNotCallRealDisposeIfSessionHasNotBeenUsed()
        {
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);

            context.Dispose();
            context.Dispose();

            session.DidNotReceive().Complete();
        }

        [Test]
        public virtual void ShouldThrowExceptionIfCompleteCalledAfterDispose()
        {
            Assert.That(() =>
            {

                var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
                var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
                var policy = new ImmediateTerminationPolicy();

                var context = new DataContext(config, sessionBuilder, policy);
                Awaken(context);
                context.Dispose();
                context.Complete();
            }, Throws.Exception.With.Message.EqualTo("Data context has already been completed without success."));

            session.Received(1).Dispose();
            session.Received(0).Complete();
        }

        [Test]
        public virtual void ShouldRaiseFinishedEventDuringDispose()
        {
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            bool? success = null;

            var context = new DataContext(config, sessionBuilder, policy);

            Awaken(context);

            context.Finished += isSuccessfully => { success = isSuccessfully; session.Received(0).Dispose(); };

            context.Dispose();

            session.Received(1).Dispose();
            success.Should().Not.Be(null);
            success.Should().Be.EqualTo(false);
            
            session.ClearReceivedCalls();

            success = null;

            context = new DataContext(config, sessionBuilder, policy);

            Awaken(context);

            context.Finished += isSuccessfully => { success = isSuccessfully; session.Received(0).Dispose(); session.Received(1).Complete(); };
            context.Complete();
            context.Dispose();
            success.Should().Not.Be(null);
            success.Should().Be.EqualTo(true);
            session.Received(1).Dispose();
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
            var config = new UnitOfWorkConfig(null, IsolationLevel.ReadCommitted, Require.New);
            var sessionBuilder = new Lazy<IOrmSession>(() => session, false);
            var policy = new ImmediateTerminationPolicy();

            var context = new DataContext(config, sessionBuilder, policy);
            context.Dispose();
            Awaken(context);
        }
    }
}