#region License

// Copyright 2009-2013 Nikita Govorov
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Domain.Event;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class EventAggregatorFixture
    {
        [Test]
        public virtual void LocalInstanceShouldBeCreatedPerContext()
        {
            var agg1 = EventAggregator.Local;
            IEventAggregator agg2 = null;

            var task = Task.Factory.StartNew(() => { agg2 = EventAggregator.Local; });

            Task.WaitAll(task);

            var agg3 = EventAggregator.Local;

            agg1.Should().Not.Be.Null();
            agg2.Should().Not.Be.Null();
            agg3.Should().Not.Be.Null();

            agg1.Should().Not.Be.SameInstanceAs(agg2);
            agg1.Should().Be.SameInstanceAs(agg3);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeCreatedPerApplication()
        {
            var agg1 = EventAggregator.Global;
            IEventAggregator agg2 = null;
            var task = Task.Factory.StartNew(() => { agg2 = EventAggregator.Global; });
            Task.WaitAll(task);
            agg1.Should().Be.SameInstanceAs(agg2);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeDefault()
        {
            var callCounter = 0;

            using (EventAggregator.Subscribe<SystemEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Local.Publish(new SystemEvent());
            }

            callCounter.Should().Be(2);
        }

        [Test]
        public virtual void GlobalInstanceShouldBeMultiThreadSafe()
        {
            var unsubscribeActions = new ConcurrentBag<Action>();

            try
            {
                Exception exception = null;

                var subscribeTasks = new List<Task>();
                var publishTasks = new List<Task>();


                //start many publishers
                for (var i = 0; i < 250; i++)
                {
                    publishTasks.Add(Task.Factory.StartNew(() => EventAggregator.Publish(new SystemEvent())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                //start many subscribers
                for (var i = 0; i < 200; i++)
                {
                    subscribeTasks.Add(Task.Factory.StartNew(() =>
                    {
                        var unsubscribe = EventAggregator.Subscribe<IEvent>(ev => { });
                        unsubscribeActions.Add(unsubscribe);
                    }).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                //start many publishers
                for (var i = 0; i < 250; i++)
                {
                    publishTasks.Add(Task.Factory.StartNew(() => EventAggregator.Publish(new ModuleEvent())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                }

                Task.WaitAll(subscribeTasks.Union(publishTasks).ToArray());

                exception.Should().Be.Null();

                subscribeTasks.Clear();
                publishTasks.Clear();

                var unsubscribTasks = new List<Task>();

                foreach (var unsubscribeAction in new List<Action>(unsubscribeActions))
                {
                    var action = unsubscribeAction;
                    //start publisher
                    publishTasks.Add(Task.Factory.StartNew(() => EventAggregator.Publish(new ModuleEvent())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                    //start unsubscriber
                    unsubscribTasks.Add(Task.Factory.StartNew(action).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                    //start publisher
                    publishTasks.Add(Task.Factory.StartNew(() => EventAggregator.Publish(new SystemEvent())).ContinueWith(t => exception = t.Exception, TaskContinuationOptions.None));
                    //start subscriber
                    subscribeTasks.Add(Task.Factory.StartNew(() =>
                    {
                        var unsubscribe = EventAggregator.Subscribe<IEvent>(ev => { });
                        unsubscribeActions.Add(unsubscribe);
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
                ((IDisposable)EventAggregator.Global).Dispose();
            }
        }

        // ReSharper disable AccessToModifiedClosure
        [Test]
        public virtual void BasicSubscribeUnsubsribeShouldWork()
        {
            var callCounter = 0;

            using (EventAggregator.Subscribe<SystemEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Local.Publish(new SystemEvent());
            }
            callCounter.Should().Be(2);

            EventAggregator.Publish(new SystemEvent());

            callCounter.Should().Be(2);

            callCounter = 0;

            EventAggregator.Subscribe<SystemEvent>(ev => callCounter++);
            EventAggregator.Subscribe<SystemEvent>(ev => callCounter++);
            ((IDisposable)EventAggregator.Global).Dispose();
            EventAggregator.Publish(new SystemEvent());
            callCounter.Should().Be(0);

            using (EventAggregator.Local.Subscribe<SystemEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new SystemEvent()); //+1

                EventAggregator.Local.Publish(new SystemEvent()); //+1    
            }
            callCounter.Should().Be(3);

            EventAggregator.Publish(new SystemEvent());

            callCounter.Should().Be(3);

            callCounter = 0;

            EventAggregator.Local.Subscribe<SystemEvent>(ev => callCounter++);
            EventAggregator.Local.Subscribe<SystemEvent>(ev => callCounter++);
            ((IDisposable)EventAggregator.Local).Dispose();
            EventAggregator.Publish(new SystemEvent());
            EventAggregator.Local.Publish(new SystemEvent());
            callCounter.Should().Be(0);

            var handler = new SystemEventHandler();
            using (EventAggregator.Subscribe(handler).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Local.Publish(new SystemEvent());
            }
            handler.CallCounter.Should().Be(2);

            EventAggregator.Publish(new SystemEvent());

            handler.CallCounter.Should().Be(2);

            handler = new SystemEventHandler();
            using (EventAggregator.OnStreamOf<SystemEvent>().Subscribe(handler).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Local.Publish(new SystemEvent());
            }
            handler.CallCounter.Should().Be(2);

            EventAggregator.Publish(new SystemEvent());

            handler.CallCounter.Should().Be(2);
        }

        [Test]
        public virtual void BasicFilteringAndProjectionShouldWork()
        {
            var callCounter = 0;

            using (EventAggregator.OnStreamOf<SystemEvent>().Where(ev => ev.HealthLevel == 100).Subscribe(ev => { callCounter++; ev.HealthLevel.Should().Be(100); }).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent(99));
                EventAggregator.Publish(new SystemEvent(100)); //+1
                EventAggregator.Publish(new SystemEvent(100)); //+1
            }
            callCounter.Should().Be(2);

            callCounter = 0;

            using (EventAggregator.OnStreamOf<SystemEvent>().Where(ev => ev.HealthLevel > 98).Select(ev => ev.HealthLevel)
                    .Where(amount => amount == 100).Subscribe(amount => { callCounter++; amount.Should().Be(100); }).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent(98));
                EventAggregator.Publish(new SystemEvent(99));
                EventAggregator.Publish(new SystemEvent(100)); //+1
                EventAggregator.Publish(new SystemEvent(100)); //+1
            }
            callCounter.Should().Be(2);

            var handler = new SystemEventHandler();

            using (EventAggregator.OnStreamOf<SystemEvent>().Where(ev => ev.HealthLevel > 98).Select(ev => ev)
                    .Where(ev => ev.HealthLevel == 100).Subscribe(handler).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent(98));
                EventAggregator.Publish(new SystemEvent(99));
                EventAggregator.Publish(new SystemEvent(100)); //+1
                EventAggregator.Publish(new SystemEvent(100)); //+1
            }
            handler.CallCounter.Should().Be(2);
        }

        [Test]
        public virtual void BasicPolymorphismShouldWork()
        {
            var callCounter = 0;

            using (EventAggregator.Subscribe<IEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new ModuleEvent()); //+1
            }
            callCounter.Should().Be(2);

            callCounter = 0;

            using (EventAggregator.Subscribe<SystemEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent()); //+1
                EventAggregator.Publish(new ModuleEvent()); //+1
            }

            callCounter.Should().Be(2);

            callCounter = 0;

            using (EventAggregator.Subscribe<ModuleEvent>(ev => callCounter++).AsDisposable())
            {
                EventAggregator.Publish(new SystemEvent());
                EventAggregator.Publish(new ModuleEvent()); //+1
            }

            callCounter.Should().Be(1);

            callCounter = 0;
        }
        
        // ReSharper restore AccessToModifiedClosure
    }
}