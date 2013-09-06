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

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Event.Internal;
using Taijutsu.Test.Data;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Event
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DeferredHandlingSettingsFixture
    {
        [SetUp]
        public void SetUp()
        {
            InternalEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));
            InternalEnvironment.RegisterDataSource(new DataSource("test", il => new SessionWithExceptionInComplete()));
        }

        [TearDown]
        public void TearDown()
        {
            InternalEnvironment.UnregisterDataSource();
            InternalEnvironment.UnregisterDataSource("test");
        }

        [Test]
        public void AllBasicParametersShouldBeSet()
        {
            var originalSettigs = new TypedHandlingSettings<SystemChecked>(() => new SpecEventHandler<SystemChecked>(ev => { }), priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.Finished);

            settigs.Type.Should().Be(typeof(SystemChecked));
            settigs.Priority.Should().Be(99);
        }

        [Test]
        public void ActionShouldBeFilteredByType()
        {
            var resolved = 0;
            var called = 0;

            var originalSettigs = new TypedHandlingSettings<SystemChecked>(
                () =>
                {
                    resolved++;
                    return new SpecEventHandler<SystemChecked>(ev => { called++; });
                }, 
                priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.PreCompleted);

            using (var uow = new UnitOfWork())
            {
                settigs.Action(new object());
                settigs.Action(new object());
                resolved.Should().Be(0);
                called.Should().Be(0);
                uow.Complete();
                resolved.Should().Be(0);
                called.Should().Be(0);
            }

            resolved.Should().Be(0);
            called.Should().Be(0);

            using (var uow = new UnitOfWork())
            {
                settigs.Action(new SystemChecked());
                settigs.Action(new SystemChecked());
                resolved.Should().Be(0);
                called.Should().Be(0);
                uow.Complete();
                resolved.Should().Be(2);
                called.Should().Be(2);
            }

            resolved.Should().Be(2);
            called.Should().Be(2);
        }

        [Test]
        public void SubscriberShouldBeIgnoredIfIsUsedOutsideOfUnitOfWork()
        {
            var resolved = false;
            var called = false;

            var originalSettigs = new TypedHandlingSettings<SystemChecked>(
                () =>
                {
                    resolved = true;
                    return new SpecEventHandler<SystemChecked>(ev => { called = true; });
                }, 
                priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.Finished);

            settigs.Action(new SystemChecked());
            resolved.Should().Be.False();
            called.Should().Be.False();
        }

        [Test]
        public void OriginalActionShouldBeCalledBeforeUnitOfWorkCompletionIfAppropriateStageIsSpecified()
        {
            var resolved = 0;
            var called = 0;

            var originalSettigs = new TypedHandlingSettings<SystemChecked>(
                () =>
                {
                    resolved++;
                    return new SpecEventHandler<SystemChecked>(ev => { called++; });
                }, 
                priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.PreCompleted);

            using (var uow = new UnitOfWork())
            {
                settigs.Action(new SystemChecked());
                settigs.Action(new SystemChecked());
                resolved.Should().Be(0);
                called.Should().Be(0);
                uow.Complete();
                resolved.Should().Be(2);
                called.Should().Be(2);
            }

            resolved.Should().Be(2);
            called.Should().Be(2);
        }

        [Test]
        public void OriginalActionShouldBeCalledAfterUnitOfWorkCompletionIfAppropriateStageIsSpecified()
        {
            var resolved = 0;
            var called = 0;

            InternalEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));

            var originalSettigs = new TypedHandlingSettings<SystemChecked>(
                () =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resolved++;

                    // ReSharper disable once AccessToModifiedClosure
                    return new SpecEventHandler<SystemChecked>(ev => { called++; });
                }, 
                priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.Completed);

            using (var uow = new UnitOfWork())
            {
                settigs.Action(new SystemChecked());
                settigs.Action(new SystemChecked());
                resolved.Should().Be(0);
                called.Should().Be(0);
                uow.Complete();
                resolved.Should().Be(2);
                called.Should().Be(2);
            }

            resolved.Should().Be(2);
            called.Should().Be(2);

            resolved = 0;
            called = 0;

            try
            {
                using (var uow = new UnitOfWork("test"))
                {
                    // ReSharper disable once UnusedVariable
                    var session = ((IWrapper)uow).WrappedObject;

                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    resolved.Should().Be(0);
                    called.Should().Be(0);
                    uow.Complete();
                }
            }
            catch (NotSupportedException)
            {
            }

            resolved.Should().Be(0);
            called.Should().Be(0);
        }

        [Test]
        public void OriginalActionShouldBeCalledWhenUnitOfWorkScopeFinishedSuccessfullyIfAppropriateStageIsSpecified()
        {
            var resolved = 0;
            var called = 0;

            var originalSettigs = new TypedHandlingSettings<SystemChecked>(
                () =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resolved++;

                    // ReSharper disable once AccessToModifiedClosure
                    return new SpecEventHandler<SystemChecked>(ev => { called++; });
                }, 
                priority: 99);

            var settigs = new DeferredHandlingSettings(originalSettigs, DelayUntil.Finished);

            using (var uow = new UnitOfWork())
            {
                settigs.Action(new SystemChecked());
                settigs.Action(new SystemChecked());
                resolved.Should().Be(0);
                called.Should().Be(0);
                uow.Complete();
                resolved.Should().Be(0);
                called.Should().Be(0);
            }

            resolved.Should().Be(2);
            called.Should().Be(2);

            resolved = 0;
            called = 0;

            try
            {
                using (var uow = new UnitOfWork("test"))
                {
                    // ReSharper disable once UnusedVariable
                    var session = ((IWrapper)uow).WrappedObject;

                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    resolved.Should().Be(0);
                    called.Should().Be(0);
                    uow.Complete();
                }
            }
            catch (NotSupportedException)
            {
            }

            resolved.Should().Be(0);
            called.Should().Be(0);
        }

        private class SessionWithExceptionInComplete : NullDataSession
        {
            public override void Complete()
            {
                throw new NotSupportedException();
            }
        }
    }
}