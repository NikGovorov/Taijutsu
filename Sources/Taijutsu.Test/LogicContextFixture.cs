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

using System.Threading;

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

namespace Taijutsu.Test
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LogicContextFixture
    {
        [TearDown]
        public void OnTearDown()
        {
            LogicContext.Reset();
        }

        [Test]
        public virtual void NotExistingDataShouldNotBeAccessible()
        {
            Assert.IsNull(LogicContext.FindData("Test"));
        }

        [Test]
        public virtual void ExistingDataShouldBeAccessible()
        {
            var obj = new object();
            LogicContext.SetData("Test2", obj);
            Assert.AreSame(obj, LogicContext.FindData("Test2"));
            LogicContext.ReleaseData("Test2");
        }

        [Test]
        public virtual void ReleasedDataShouldNotBeAccessible()
        {
            var obj = new object();
            LogicContext.SetData("Test3", obj);
            Assert.AreSame(obj, LogicContext.FindData("Test3"));
            LogicContext.ReleaseData("Test3");
            Assert.IsNull(LogicContext.FindData("Test3"));
        }

        [Test]
        public virtual void ShouldAutomaticallyReleaseDataWhenThreadIsReturnedToThePool()
        {
            try
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(1, 1);
                var mainEvent = new AutoResetEvent(false);
                int? threadId = null;
                int? otherThreadId = null;
                object data = null;
                ThreadPool.QueueUserWorkItem(
                    delegate(object ev)
                    {
                        LogicContext.SetData("Test4", 100);
                        threadId = Thread.CurrentThread.ManagedThreadId;
                        ((AutoResetEvent)ev).Set();
                    }, 
                    mainEvent);

                mainEvent.WaitOne();

                mainEvent.Reset();

                Assert.IsNotNull(threadId);

                ThreadPool.QueueUserWorkItem(
                    delegate(object ev)
                    {
                        otherThreadId = Thread.CurrentThread.ManagedThreadId;
                        data = LogicContext.FindData("Test4");
                        ((AutoResetEvent)ev).Set();
                    }, 
                    mainEvent);
                mainEvent.WaitOne();

                Assert.IsNotNull(otherThreadId);
                Assert.AreEqual(threadId.Value, otherThreadId.Value);
                Assert.IsNull(data);
            }
            finally
            {
                ThreadPool.SetMinThreads(2, 2);
                ThreadPool.SetMaxThreads(1023, 1000);
            }
        }

        [Test]
        public virtual void ShouldBeCustomizable()
        {
            var data = new object();
            var context = Substitute.For<ILogicContext>();
            context.FindData("Test").Returns(data);

            LogicContext.Customize(context);

            LogicContext.SetData("Test", data);
            LogicContext.FindData("Test").Should().Be.SameInstanceAs(data);
            LogicContext.ReleaseData("Test");

            context.Received(1).FindData("Test");
            context.Received(1).ReleaseData("Test");
            context.Received(1).SetData("Test", data);

            Assert.That(() => LogicContext.Customize(context), Throws.Exception);
        }
    }
}