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
using System.Collections.Generic;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Event;
using Taijutsu.Event.Internal;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Event
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DynamicHandlingSettingsFixture
    {
        [Test]
        public void AllBasicParametersShouldBeSet()
        {
            Func<IEnumerable<Func<object>>> resolver = () => new Func<object>[] { () => new object() };

            var settigs = new DynamicHandlingSettings(typeof(ModuleChecked), resolver, 200);

            settigs.Type.Should().Be(typeof(ModuleChecked));
            settigs.Priority.Should().Be(200);
        }

        [Test]
        public void ActionShouldBeFilteredByHandlerResolversLength()
        {
            Func<IEnumerable<Func<object>>> resolver = () => new Func<object>[0];

            var settigs = new DynamicHandlingSettings(typeof(ModuleChecked), resolver, 200);

            settigs.Action(new object());
        }

        [Test]
        public void ActionShouldBeFilteredByType()
        {
            var called = false;
            var resolved = false;
            Func<IEnumerable<Func<object>>> resolver = () => new[]
                                                             {
                                                                 (Func<object>)(() =>
                                                                 {
                                                                     resolved = true;
                                                                     return new SpecEventHandler<SystemChecked>(mce => { called = true; });
                                                                 })
                                                             };

            var settigs = new DynamicHandlingSettings(typeof(SystemChecked), resolver, 200);

            settigs.Action(new object());

            resolved.Should().Be.False();
            called.Should().Be.False();
        }

        [Test]
        public void ActionShouldCallAppropriateMethods()
        {
            var firstResolved = false;
            var firstHandleCalled = false;
            var firstFilterCalled = false;

            var secondResolved = false;
            var secondHandleCalled = false;
            var secondFilterCalled = false;

            var thirdResolved = false;
            var thirdHandleCalled = false;

            var fourthResolved = false;

            var allInOneHandler = new AllInOneEventHandler();

            Func<IEnumerable<Func<object>>> resolver = () => new[]
                                                             {
                                                                 (Func<object>)(() =>
                                                                 {
                                                                     firstResolved = true;
                                                                     return new SpecEventHandler<SystemChecked>(
                                                                         mce => { firstHandleCalled = true; }, 
                                                                         mce =>
                                                                         {
                                                                             firstFilterCalled = true;
                                                                             return true;
                                                                         });
                                                                 }), 
                                                                 (Func<object>)(() =>
                                                                 {
                                                                     secondResolved = true;
                                                                     return new SpecEventHandler<SystemChecked>(
                                                                         mce => { secondHandleCalled = true; }, 
                                                                         mce =>
                                                                         {
                                                                             secondFilterCalled = true;
                                                                             return false;
                                                                         });
                                                                 }), 
                                                                 (Func<object>)(() =>
                                                                 {
                                                                     thirdResolved = true;
                                                                     return new TestGenericEventHandler<SystemChecked>(mce => { thirdHandleCalled = true; });
                                                                 }), 
                                                                 (Func<object>)(() =>
                                                                 {
                                                                     fourthResolved = true;
                                                                     // ReSharper disable once AccessToModifiedClosure
                                                                     return allInOneHandler;
                                                                 })
                                                             };

            var settigs = new DynamicHandlingSettings(typeof(SystemChecked), resolver, 200);

            settigs.Action(new ModuleChecked());

            firstResolved.Should().Be.True();
            firstFilterCalled.Should().Be.True();
            firstHandleCalled.Should().Be.True();

            secondResolved.Should().Be.True();
            secondFilterCalled.Should().Be.True();
            secondHandleCalled.Should().Be.False();

            thirdResolved.Should().Be.True();
            thirdHandleCalled.Should().Be.True();

            fourthResolved.Should().Be.True();

            allInOneHandler.SystemCheckedHandleCalled.Should().Be.True();
            allInOneHandler.SystemCheckedIsSatisfiedByThroughInterfaceCalled.Should().Be.True();
            allInOneHandler.SystemCheckedIsSatisfiedByCalled.Should().Be.False();

            allInOneHandler.ModuleCheckedHandleCalled.Should().Be.False();
            allInOneHandler.ModuleCheckedIsSatisfiedByCalled.Should().Be.False();

            allInOneHandler = new AllInOneEventHandler();

            resolver = () => new[]
                             {
                                 (Func<object>)(() => allInOneHandler)
                             };

            settigs = new DynamicHandlingSettings(typeof(ModuleChecked), resolver, 200);

            settigs.Action(new ModuleChecked());

            allInOneHandler.ModuleCheckedHandleCalled.Should().Be.True();
            allInOneHandler.ModuleCheckedIsSatisfiedByCalled.Should().Be.False();

            allInOneHandler.SystemCheckedHandleCalled.Should().Be.False();
            allInOneHandler.SystemCheckedIsSatisfiedByThroughInterfaceCalled.Should().Be.False(); // should not be covariant.
            allInOneHandler.SystemCheckedIsSatisfiedByCalled.Should().Be.False();
        }

        [Test]
        public void ActionShouldBeSafeToNull()
        {
            var resolved = false;
            Func<IEnumerable<Func<object>>> resolver = () => new[] { (Func<object>)null };

            var settigs = new DynamicHandlingSettings(typeof(SystemChecked), resolver, 200);

            settigs.Action(new SystemChecked());

            resolver = () => new[]
                             {
                                 (Func<object>)(() =>
                                 {
                                     resolved = true;
                                     return null;
                                 })
                             };

            settigs = new DynamicHandlingSettings(typeof(SystemChecked), resolver, 200);

            settigs.Action(new SystemChecked());

            resolved.Should().Be.True();
        }

        private class TestGenericEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
        {
            private readonly Action<TEvent> action;

            public TestGenericEventHandler(Action<TEvent> action)
            {
                this.action = action;
            }

            public void Handle(TEvent subject)
            {
                action(subject);
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private class AllInOneEventHandler : ISpecEventHandler<SystemChecked>, IEventHandler<ModuleChecked>
        {
            public bool SystemCheckedHandleCalled { get; set; }

            public bool SystemCheckedIsSatisfiedByThroughInterfaceCalled { get; set; }

            public bool SystemCheckedIsSatisfiedByCalled { get; set; }

            public bool ModuleCheckedHandleCalled { get; set; }

            public bool ModuleCheckedIsSatisfiedByCalled { get; set; }

            public void Handle(SystemChecked subject)
            {
                SystemCheckedHandleCalled = true;
            }

            bool ISpecHandler<SystemChecked>.IsSatisfiedBy(SystemChecked candidate)
            {
                SystemCheckedIsSatisfiedByThroughInterfaceCalled = true;
                return true;
            }

            public void Handle(ModuleChecked subject)
            {
                ModuleCheckedHandleCalled = true;
            }

            public bool IsSatisfiedBy(SystemChecked candidate)
            {
                SystemCheckedIsSatisfiedByCalled = false;
                return true;
            }

            public bool IsSatisfiedBy(ModuleChecked candidate)
            {
                ModuleCheckedIsSatisfiedByCalled = true;
                return true;
            }
        }
    }
}