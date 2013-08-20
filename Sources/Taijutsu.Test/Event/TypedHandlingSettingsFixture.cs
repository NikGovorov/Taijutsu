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

using Taijutsu.Annotation;
using Taijutsu.Event.Internal;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Event
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TypedHandlingSettingsFixture
    {
        [Test]
        public void AllBasicParametersShouldBeSet()
        {
            var filters = new List<Func<ModuleChecked, bool>> { ev => true, ev => false };
            var settigs = new TypedHandlingSettings<ModuleChecked>(() => new SpecEventHandler<ModuleChecked>(ev => { }), filters, 100);
            settigs.Type.Should().Be(typeof(ModuleChecked));
            settigs.Priority.Should().Be(100);
            settigs.Filters.Should().Have.SameSequenceAs(filters);
        }

        [Test]
        public void ActionShouldBeFilteredByType()
        {
            var resolved = false;

            var settigs = new TypedHandlingSettings<ModuleChecked>(
                () =>
                {
                    resolved = true;
                    return new SpecEventHandler<ModuleChecked>(ev => { });
                });

            settigs.Action(new object());
            resolved.Should().Be.False();
        }

        [Test]
        public void ActionShouldBeFilteredByFilters()
        {
            var resolved = false;
            var called = false;
            var filters = new List<Func<ModuleChecked, bool>> { ev => ev.HealthLevel > 98, ev => ev.HealthLevel < 100 };

            var settigs = new TypedHandlingSettings<ModuleChecked>(
                () =>
                {
                    resolved = true;
                    return new SpecEventHandler<ModuleChecked>(ev => { called = true; });
                }, 
                filters);

            settigs.Action(new ModuleChecked(98));
            resolved.Should().Be.False();
            settigs.Action(new ModuleChecked(100));
            resolved.Should().Be.False();
            settigs.Action(new ModuleChecked(99));
            resolved.Should().Be.True();
            called.Should().Be.True();
        }

        [Test]
        public void ActionShouldBeFilteredByHandlerSpecification()
        {
            var resolved = false;
            var called = false;
            var filters = new List<Func<ModuleChecked, bool>> { ev => ev.HealthLevel > 97, ev => ev.HealthLevel < 100 };

            var settigs = new TypedHandlingSettings<ModuleChecked>(
                () =>
                {
                    resolved = true;
                    return new SpecEventHandler<ModuleChecked>(ev => { called = true; }, ev => ev.HealthLevel == 99);
                }, 
                filters);

            settigs.Action(new ModuleChecked(98));
            resolved.Should().Be.True();
            called.Should().Be.False();
            settigs.Action(new ModuleChecked(99));
            resolved.Should().Be.True();
            called.Should().Be.True();
        }

        [Test]
        public void ActionShouldBeSafeToNull()
        {
            var resolved = false;

            var settigs = new TypedHandlingSettings<ModuleChecked>(
                () =>
                {
                    resolved = true;
                    return null;
                });

            settigs.Action(new ModuleChecked(98));
            resolved.Should().Be.True();
        }
    }
}