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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Event;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Event
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EventsFixture
    {
        [Test]
        public virtual void LocalInstanceShouldBeCreatedPerContext()
        {
            var agg1 = Events.Local;
            IEvents agg2 = null;

            var task = Task.Factory.StartNew(() => { agg2 = Events.Local; });

            Task.WaitAll(task);

            var agg3 = Events.Local;

            agg1.Should().Not.Be.Null();
            agg2.Should().Not.Be.Null();
            agg3.Should().Not.Be.Null();

            agg1.Should().Not.Be.SameInstanceAs(agg2);
            agg1.Should().Be.SameInstanceAs(agg3);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeCreatedPerApplication()
        {
            var agg1 = Events.Global;
            IEvents agg2 = null;
            var task = Task.Factory.StartNew(() => { agg2 = Events.Global; });
            Task.WaitAll(task);
            agg1.Should().Be.SameInstanceAs(agg2);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeDefault()
        {
            var callCounter = 0;

            using (Events.Subscribe<SystemChecked>(ev => callCounter++))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new SystemChecked()); // +1
                Events.Local.Publish(new SystemChecked());
            }

            callCounter.Should().Be(2);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeThreadSafe()
        {
            var unsubscribeActions = new ConcurrentBag<Action>();

            try
            {
                Exception exception = null;

                var subscribeTasks = new List<Task>();
                var publishTasks = new List<Task>();

                // start many publishers
                for (var i = 0; i < 250; i++)
                {
                    publishTasks.Add(Task.Factory.StartNew(() => Events.Publish(new SystemChecked())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                // start many subscribers
                for (var i = 0; i < 200; i++)
                {
                    subscribeTasks.Add(
                        Task.Factory.StartNew(
                            () =>
                            {
                                var unsubscribe = Events.Subscribe<IEvent>(ev => { });
                                unsubscribeActions.Add(unsubscribe.Dispose);
                            }).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                // start many publishers
                for (var i = 0; i < 250; i++)
                {
                    publishTasks.Add(Task.Factory.StartNew(() => Events.Publish(new ModuleChecked())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                Task.WaitAll(subscribeTasks.Union(publishTasks).ToArray());

                exception.Should().Be.Null();

                subscribeTasks.Clear();
                publishTasks.Clear();

                var unsubscribTasks = new List<Task>();

                foreach (var unsubscribeAction in new List<Action>(unsubscribeActions))
                {
                    var action = unsubscribeAction;

                    // start publisher
                    publishTasks.Add(Task.Factory.StartNew(() => Events.Publish(new ModuleChecked())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));

                    // start unsubscriber
                    unsubscribTasks.Add(Task.Factory.StartNew(action).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));

                    // start publisher
                    publishTasks.Add(Task.Factory.StartNew(() => Events.Publish(new SystemChecked())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));

                    // start subscriber
                    subscribeTasks.Add(
                        Task.Factory.StartNew(
                            () =>
                            {
                                var unsubscribe = Events.Subscribe<IEvent>(ev => { });
                                unsubscribeActions.Add(unsubscribe.Dispose);
                            }).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                Task.WaitAll(subscribeTasks.Union(publishTasks).Union(unsubscribTasks).ToArray());

                subscribeTasks.Clear();
                publishTasks.Clear();
                unsubscribTasks.Clear();

                exception.Should().Be.Null();
            }
            finally
            {
                foreach (var action in unsubscribeActions)
                {
                    action();
                }

                Events.Global.Reset();
            }
        }

        // ReSharper disable AccessToModifiedClosure
        [Test]
        public virtual void BasicSubscribeUnsubsribeShouldWork()
        {
            var callCounter = 0;

            using (Events.OfType<SystemChecked>().Subscribe(ev => callCounter++))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new SystemChecked()); // +1
                Events.Local.Publish(new SystemChecked());
            }

            callCounter.Should().Be(2);

            Events.Publish(new SystemChecked());

            callCounter.Should().Be(2);

            callCounter = 0;

            using (Events.Local.OfType<SystemChecked>().Subscribe(ev => callCounter++))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new SystemChecked()); // +1

                Events.Local.Publish(new SystemChecked()); // +1    
            }

            callCounter.Should().Be(3);

            Events.Publish(new SystemChecked());

            callCounter.Should().Be(3);

            callCounter = 0;

            var handler = new SystemCheckedHandler();
            using (Events.OfAnyType().Subscribe<SystemChecked>(handler.Handle))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new SystemChecked()); // +1
                Events.Local.Publish(new SystemChecked());
            }

            handler.CallCounter.Should().Be(2);

            Events.Publish(new SystemChecked());

            handler.CallCounter.Should().Be(2);

            handler = new SystemCheckedHandler();
            using (Events<SystemChecked>.Subscribe(handler.Handle))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new SystemChecked()); // +1
                Events.Local.Publish(new SystemChecked());
            }

            handler.CallCounter.Should().Be(2);

            Events.Publish(new SystemChecked());

            handler.CallCounter.Should().Be(2);
        }

        [Test]
        public virtual void ResetShouldRemoveAllSubscriptions()
        {
            var callCounter = 0;

            Events.Subscribe<SystemChecked>(ev => callCounter++);
            Events.Subscribe<SystemChecked>(ev => callCounter++);

            Events.Global.Reset();
            Events.Publish(new SystemChecked());
            callCounter.Should().Be(0);

            Events.Local.Subscribe<SystemChecked>(ev => callCounter++);
            Events.Local.Subscribe<SystemChecked>(ev => callCounter++);

            Events.Local.Reset();
            Events.Publish(new SystemChecked());
            Events.Local.Publish(new SystemChecked());
            callCounter.Should().Be(0);
        }

        [Test]
        public virtual void BasicFilteringShouldWork()
        {
            var callCounter = 0;

            using (Events<SystemChecked>.Where(ev => ev.HealthLevel == 100).Subscribe(
                ev =>
                {
                    callCounter++;
                    ev.HealthLevel.Should().Be(100);
                }))
            {
                Events.Publish(new SystemChecked(99));
                Events.Publish(new SystemChecked(100)); // +1
                Events.Publish(new SystemChecked(100)); // +1
            }

            callCounter.Should().Be(2);

            callCounter = 0;

            using (Events<SystemChecked>.Where(ev => ev.HealthLevel > 98)
                                        .Where(ev => ev.HealthLevel == 100).Subscribe(
                                            ev =>
                                            {
                                                callCounter++;
                                                ev.HealthLevel.Should().Be(100);
                                            }))
            {
                Events.Publish(new SystemChecked(98));
                Events.Publish(new SystemChecked(99));
                Events.Publish(new SystemChecked(100)); // +1
                Events.Publish(new SystemChecked(100)); // +1
            }

            callCounter.Should().Be(2);

            var handler = new SystemCheckedHandler();

            using (Events<SystemChecked>.Where(ev => ev.HealthLevel > 98)
                                        .Where(ev => ev.HealthLevel == 100).Subscribe(handler.Handle))
            {
                Events.Publish(new SystemChecked(98));
                Events.Publish(new SystemChecked(99));
                Events.Publish(new SystemChecked(100)); // +1
                Events.Publish(new SystemChecked(100)); // +1
            }

            handler.CallCounter.Should().Be(2);
        }

        [Test]
        public virtual void BasicInheritanceShouldWork()
        {
            var callCounter = 0;

            using (Events.Subscribe<IEvent>(ev => callCounter++))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new ModuleChecked()); // +1
            }

            callCounter.Should().Be(2);

            callCounter = 0;

            using (Events.Subscribe<SystemChecked>(ev => callCounter++))
            {
                Events.Publish(new SystemChecked()); // +1
                Events.Publish(new ModuleChecked()); // +1
            }

            callCounter.Should().Be(2);

            callCounter = 0;

            using (Events.Subscribe<ModuleChecked>(ev => callCounter++))
            {
                Events.Publish(new SystemChecked());
                Events.Publish(new ModuleChecked()); // +1
            }

            callCounter.Should().Be(1);

            callCounter = 0;
        }

        // ReSharper restore AccessToModifiedClosure
    }
}