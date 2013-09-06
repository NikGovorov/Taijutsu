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

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DataSessionBuilderFixture
    {
        [Test]
        public virtual void ShouldSetNameCorrectly()
        {
            var builder = new DataSessionBuilder(options => new NullDataSession(), "Test");
            builder.BuildSession();
            builder.Name.Should().Be("Test");

            builder = new DataSessionBuilder(() => new NullDataSession(), "Test");
            builder.BuildSession();
            builder.Name.Should().Be("Test");

            builder = new DataSessionBuilder<NullDataSession>(() => new NullDataSession(), "Test");
            builder.BuildSession();
            builder.Name.Should().Be("Test");

            builder = new DataSessionBuilder<NullDataSession>(options => new NullDataSession());
            builder.BuildSession();
            builder.Name.Should().Be(typeof(NullDataSession).FullName);

            builder = new DataSessionBuilder<NullDataSession>(() => new NullDataSession());
            builder.BuildSession();
            builder.Name.Should().Be(typeof(NullDataSession).FullName);
        }

        [Test]
        public virtual void ShouldPassOptionsToFactoryMethodAndReturnExpectedSession()
        {
            var actualSession = new NullDataSession();
            var actualOptions = new { };
            var called = false;
            var builder = new DataSessionBuilder<NullDataSession>(
                options =>
                {
                    options.Should().Be(actualOptions);
                    called = true;
                    return actualSession;
                });
            builder.BuildSession(actualOptions).Should().Be.SameInstanceAs(actualSession);
            called.Should().Be.True();
        }

        [Test]
        public virtual void FactoryMethodShouldBeExecutedInsideConstructionScope()
        {
            var called = false;

            var builder = new DataSessionBuilder<NullDataSession>(
                options =>
                {
                    InternalEnvironment.IsInsideConstructionScope.Should().Be.True();
                    called = true;
                    return new NullDataSession();
                });

            InternalEnvironment.IsInsideConstructionScope.Should().Be.False();
            builder.BuildSession();
            InternalEnvironment.IsInsideConstructionScope.Should().Be.False();

            called.Should().Be.True();
        }
    }
}