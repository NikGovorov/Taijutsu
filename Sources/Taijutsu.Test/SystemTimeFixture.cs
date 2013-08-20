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
using System.Threading;

using NUnit.Framework;

using Taijutsu.Annotation;

namespace Taijutsu.Test
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SystemTimeFixture
    {
        [TearDown]
        public void OnTearDown()
        {
            SystemTime.Reset();
        }

        [SetUp]
        public void OnSetUp()
        {
            SystemTime.TimeController.Reset();
        }

        [Test]
        public virtual void NotCustomizedTimeShouldReturnCurrentUtcTime()
        {
            var real = DateTime.UtcNow;
            var system = SystemTime.Now;
            Assert.That(real, Is.EqualTo(system).Within(3).Milliseconds);
        }

        [Test]
        public virtual void FrozenSystemTimeShouldBePaused()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            var frozen = DateTime.MinValue.AddDays(1);
            SystemTime.TimeController.SetFrozenDate(frozen);
            Assert.AreNotEqual(DateTime.UtcNow, SystemTime.Now);
            Assert.AreEqual(frozen, SystemTime.Now);
        }

        [Test]
        public virtual void SetTimeShouldBeLive()
        {
            var startPoint = new DateTime(2010, 01, 01, 23, 59, 59, 995);
            SystemTime.TimeController.SetDate(startPoint);
            Thread.Sleep(50);
            var now = SystemTime.Now;
            Assert.That(new DateTime(2010, 01, 02, 0, 0, 0, 45), Is.EqualTo(now).Within(30).Milliseconds);
        }

        [Test]
        public virtual void ResetedTimeShouldReturnCurrentUtcTime()
        {
            SystemTime.TimeController.SetFrozenDate(DateTime.MinValue);
            Assert.AreNotEqual(DateTime.UtcNow, SystemTime.Now);
            SystemTime.TimeController.Reset();
            Assert.That(DateTime.UtcNow, Is.EqualTo(SystemTime.Now).Within(3).Milliseconds);

            SystemTime.TimeController.SetFrozenDate(DateTime.MinValue);
            Assert.AreNotEqual(DateTime.UtcNow, SystemTime.Now);
            SystemTime.Reset();
            Assert.That(DateTime.UtcNow, Is.EqualTo(SystemTime.Now).Within(3).Milliseconds);
        }

        [Test]
        public virtual void CustomizedTimeShouldReturnValueFromCustomizationFunction()
        {
            SystemTime.TimeController.Customize(() => DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, SystemTime.Now);
        }

        [Test]
        public virtual void ShouldAllowToSetCustomTimeController()
        {
            SystemTime.TimeController = new LocalTimeController();
            var real = DateTime.Now;
            var system = SystemTime.Now;
            Assert.That(real, Is.EqualTo(system).Within(3).Milliseconds);
        }

        internal class LocalTimeController : ITimeController
        {
            private Func<DateTime> nowFunc = () => DateTime.Now;

            public DateTime Now
            {
                get { return nowFunc(); }
            }

            public void Customize(Func<DateTime> func)
            {
                nowFunc = func;
            }

            public void SetDate(DateTime date)
            {
                var whnStd = DateTime.Now;
                Func<DateTime> func = () => date + (DateTime.Now - whnStd);
                nowFunc = func;
            }

            public void SetFrozenDate(DateTime date)
            {
                nowFunc = () => date;
            }

            public void Reset()
            {
                nowFunc = () => DateTime.Now;
            }
        }
    }
}