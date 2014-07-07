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

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Event;
using Taijutsu.Event.Internal;
using Taijutsu.Test.Data;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Event
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BatchedHandlingSettingsFixture
    {
        [SetUp]
        public void SetUp()
        {
            DataEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));
            DataEnvironment.RegisterDataSource(new DataSource("test", il => new SessionWithExceptionInComplete()));
        }

        [TearDown]
        public void TearDown()
        {
            DataEnvironment.UnregisterDataSource();
            DataEnvironment.UnregisterDataSource("test");
            Events.Global.Reset();
        }

        [Test]
        public void AllBasicParametersShouldBeSet()
        {
            var settigs = new BatchedHandlingSettings(typeof(ModuleChecked), DelayUntil.Finished, 1000);

            settigs.Type.Should().Be(typeof(ModuleChecked));
            settigs.Priority.Should().Be(1000);
        }

        [Test]
        public void ActionShouldBeFilteredByType()
        {
            var called = 0;
            IEventBatch<SystemChecked> batch = null;
            using (Events.Subscribe<IEventBatch<SystemChecked>>(
                ev =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    called++;
                    batch = ev;
                }))
            {
                var settigs = new BatchedHandlingSettings(typeof(SystemChecked), DelayUntil.Finished, 1000);

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new object());
                    settigs.Action(new object());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                }

                called.Should().Be(0);
                batch.Should().Be.Null();

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                }

                called.Should().Be(1);
                batch.Should().Not.Be.Null();
                batch.Events.Should().Have.Count.EqualTo(2);

                called = 0;
                batch = null;

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new ModuleChecked());
                    settigs.Action(new ModuleChecked());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                }

                called.Should().Be(1);
                batch.Should().Not.Be.Null();
            }
        }

        [Test]
        public void EventPublishingShouldBeIgnoredIfIsUsedOutsideOfUnitOfWork()
        {
            var called = 0;

            using (Events.Subscribe<IEventBatch<ModuleChecked>>(ev => { called++; }))
            {
                var settigs = new BatchedHandlingSettings(typeof(ModuleChecked), DelayUntil.Finished, 100);
                settigs.Action(new ModuleChecked());
            }

            called.Should().Be(0);
        }

        [Test]
        public void EventBatchShouldBePublishedBeforeUnitOfWorkCompletionIfAppropriateStageIsSpecified()
        {
            var called = 0;
            IEventBatch<SystemChecked> batch = null;
            using (Events.Subscribe<IEventBatch<SystemChecked>>(
                ev =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    called++;
                    batch = ev;
                }))
            {
                var settigs = new BatchedHandlingSettings(typeof(SystemChecked), DelayUntil.PreCompleted, 1000);

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(1);
                    batch.Should().Not.Be.Null();
                    batch.Events.Should().Have.Count.EqualTo(2);
                }

                called.Should().Be(1);
                batch.Should().Not.Be.Null();
                batch.Events.Should().Have.Count.EqualTo(2);
            }
        }

        [Test]
        public void EventBatchShouldBePublishedAfterUnitOfWorkCompletionIfAppropriateStageIsSpecified()
        {
            var called = 0;
            IEventBatch<SystemChecked> batch = null;
            using (Events.Subscribe<IEventBatch<SystemChecked>>(
                ev =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    called++;
                    batch = ev;
                }))
            {
                var settigs = new BatchedHandlingSettings(typeof(SystemChecked), DelayUntil.Completed, 1000);

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(1);
                    batch.Should().Not.Be.Null();
                    batch.Events.Should().Have.Count.EqualTo(2);
                }

                called.Should().Be(1);
                batch.Should().Not.Be.Null();
                batch.Events.Should().Have.Count.EqualTo(2);

                called = 0;
                batch = null;

                try
                {
                    using (var uow = new UnitOfWork("test"))
                    {
                        // ReSharper disable once UnusedVariable
                        var session = ((IDecorator)uow).Origin;

                        settigs.Action(new SystemChecked());
                        settigs.Action(new SystemChecked());
                        called.Should().Be(0);
                        batch.Should().Be.Null();
                        uow.Complete();
                    }
                }
                catch (NotSupportedException)
                {
                }

                called.Should().Be(0);
                batch.Should().Be.Null();
            }
        }

        [Test]
        public void EventBatchShouldBePublishedWhenUnitOfWorkScopeFinishedSuccessfullyIfAppropriateStageIsSpecified()
        {
            var called = 0;
            IEventBatch<SystemChecked> batch = null;
            using (Events.Subscribe<IEventBatch<SystemChecked>>(
                ev =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    called++;
                    batch = ev;
                }))
            {
                var settigs = new BatchedHandlingSettings(typeof(SystemChecked), DelayUntil.Finished, 1000);

                using (var uow = new UnitOfWork())
                {
                    settigs.Action(new SystemChecked());
                    settigs.Action(new SystemChecked());
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                    uow.Complete();
                    called.Should().Be(0);
                    batch.Should().Be.Null();
                }

                called.Should().Be(1);
                batch.Should().Not.Be.Null();
                batch.Events.Should().Have.Count.EqualTo(2);

                called = 0;
                batch = null;

                try
                {
                    using (var uow = new UnitOfWork("test"))
                    {
                        // ReSharper disable once UnusedVariable
                        var session = ((IDecorator)uow).Origin;

                        settigs.Action(new SystemChecked());
                        settigs.Action(new SystemChecked());
                        called.Should().Be(0);
                        batch.Should().Be.Null();
                        uow.Complete();
                    }
                }
                catch (NotSupportedException)
                {
                }

                called.Should().Be(0);
                batch.Should().Be.Null();
            }
        }

        [Test]
        public void BatchedEventsHandlersShouldBeUnique()
        {
            Events.OfType<SystemChecked>().BatchUntilCompleted();
            Events.OfType<SystemChecked>().BatchUntilCompleted();
            Events.OfType<SystemChecked>().BatchUntilCompleted();
            Events.OfType<SystemChecked>().BatchUntilPreCompleted();
            Events.OfType<SystemChecked>().BatchUntilPreCompleted();
            Events.OfType<SystemChecked>().BatchUntilPreCompleted();
            Events.OfType<SystemChecked>().BatchUntilFinished();
            Events.OfType<SystemChecked>().BatchUntilFinished();
            Events.OfType<SystemChecked>().BatchUntilFinished();
            var called = 0;
            using (Events.Subscribe<IEventBatch<SystemChecked>>(
                ev =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    called++;
                }))
            {
                using (var uow = new UnitOfWork())
                {
                    Events.Publish(new SystemChecked());
                    Events.Publish(new SystemChecked());
                    Events.Publish(new SystemChecked());
                    called.Should().Be(0);
                    uow.Complete();
                    called.Should().Be(2);
                }

                called.Should().Be(3);
            }
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