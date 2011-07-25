// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Taijutsu.Domain.Specs
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class LogicalContext_Specs
    {

        [Test]
        public virtual void Logical_context_should_return_null_if_it_was_not_found()
        {
            Assert.IsNull(LogicalContext.FindData("mahdskshfd"));
        }

        [Test]
        public virtual void When_data_setted_it_should_be_accessible_through_context()
        {
            var obj = new object();
            LogicalContext.SetData("mahdskshfdsas", obj);
            Assert.AreSame(obj, LogicalContext.FindData("mahdskshfdsas"));
        }

        [Test]
        public virtual void When_data_released_it_shoild_not_be_accessible_through_context()
        {
            var obj = new object();
            LogicalContext.SetData("mahasdasddskshfdsas", obj);
            Assert.AreSame(obj, LogicalContext.FindData("mahasdasddskshfdsas"));
            LogicalContext.ReleaseData("mahasdasddskshfdsas");
            Assert.IsNull(LogicalContext.FindData("mahasdasddskshfdsas"));
        }

        [Test]
        public virtual void Default_logical_context_should_automatically_release_data()
        {
            try
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(1, 1);
                var mainEvent = new AutoResetEvent(false);
                int? threadId = null;
                int? otherThreadId = null;
                object data = null;
                ThreadPool.QueueUserWorkItem(delegate(object ev)
                                                 {
                                                     LogicalContext.SetData("test", 100);
                                                     threadId = Thread.CurrentThread.ManagedThreadId;
                                                     ((AutoResetEvent)ev).Set();
                                                 }, mainEvent);

                mainEvent.WaitOne();

                mainEvent.Reset();

                Assert.IsNotNull(threadId);

                ThreadPool.QueueUserWorkItem(delegate(object ev)
                                                 {
                                                     otherThreadId = Thread.CurrentThread.ManagedThreadId;
                                                     data = LogicalContext.FindData("test");
                                                     ((AutoResetEvent)ev).Set();
                                                 }, mainEvent);
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

    }

    // ReSharper restore InconsistentNaming
}