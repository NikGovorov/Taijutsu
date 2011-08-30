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

using System;
using System.Threading;
using NUnit.Framework;

namespace Taijutsu.Specs.Domain
{
// ReSharper disable InconsistentNaming    

    [TestFixture]
    public class SystemTime_Specs
    {
        [SetUp]
        protected void OnSetUp()
        {
            SystemTime.TimeController.Reset();
        }

        [Test]
        public virtual void When_time_is_not_customized_it_should_return_current_time()
        {
            var real = DateTime.Now;
            var system = SystemTime.Now;
            Assert.That(real, Is.EqualTo(system).Within(3).Milliseconds);
        }

        [Test]
        public virtual void When_time_has_been_frozen_system_time_should_be_paused()
        {
            var frozen  = DateTime.MinValue.AddDays(1);
            SystemTime.TimeController.SetFrozenDate(frozen);
            Assert.AreNotEqual(DateTime.Now, SystemTime.Now);
            Assert.AreEqual(frozen, SystemTime.Now);            
        }

        [Test]
        public virtual void Reset_should_set_current_time_behaviour()
        {
            SystemTime.TimeController.SetFrozenDate(DateTime.MinValue);
            Assert.AreNotEqual(DateTime.Now, SystemTime.Now);
            SystemTime.TimeController.Reset();
            Assert.That(DateTime.Now, Is.EqualTo(SystemTime.Now).Within(3).Milliseconds);
        }

        [Test]
        public virtual void Customizing_should_work()
        {
            SystemTime.TimeController.Customize(() => DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, SystemTime.Now);
        }

        [Test]
        public virtual void When_time_has_been_setted_it_should_be_live()
        {
            var startPoint = new DateTime(2010, 01, 01, 23, 59 , 59, 900);
            SystemTime.TimeController.SetDate(startPoint);
            Thread.Sleep(100);
            Assert.That(new DateTime(2010, 01, 02), Is.EqualTo(SystemTime.Now).Within(3).Milliseconds);
        }

    }

// ReSharper restore InconsistentNaming
}