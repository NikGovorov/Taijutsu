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
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DelayedTerminationPolicyFixture
    {
        [Test]
        public virtual void ShouldPostponeSessionTermination()
        {
            var session1 = Substitute.For<IOrmSession>();
            var session2 = Substitute.For<IOrmSession>();

            var policy = new DelayedTerminationPolicy();

            policy.Terminate(session1, true);
            session1.Received(0).Dispose();
            policy.Terminate(session2, true);
            session2.Received(0).Dispose();

            ((IDisposable)policy).Dispose();
            session1.Received(1).Dispose();
            session2.Received(1).Dispose();
        }

        [Test]
        public virtual void ShouldTerminateSessionImmediatelyIfPolicyDisposed()
        {
            var session1 = Substitute.For<IOrmSession>();

            var policy = new DelayedTerminationPolicy();

            ((IDisposable)policy).Dispose();

            policy.Terminate(session1, true);
            session1.Received(1).Dispose();
        }

        [Test]
        public virtual void ShouldAggregateExceptionsOnDispose()
        {
            var session1 = Substitute.For<IOrmSession>();
            var session2 = Substitute.For<IOrmSession>();
            var session3 = Substitute.For<IOrmSession>();

            session1.When(s => s.Dispose()).Do(s => { throw new Exception("session1"); });
            session3.When(s => s.Dispose()).Do(s => { throw new Exception("session3"); });

            var policy = new DelayedTerminationPolicy();

            policy.Terminate(session1, true);
            policy.Terminate(session2, true);
            policy.Terminate(session3, true);

            try
            {
                ((IDisposable)policy).Dispose();
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Message.Should().Be("session1");
                aggregateException.InnerExceptions.Any(e => e.Message == "session1").Should().Be.True();
                aggregateException.InnerExceptions.Any(e => e.Message == "session2").Should().Be.False();
                aggregateException.InnerExceptions.Any(e => e.Message == "session3").Should().Be.True();
                aggregateException.InnerExceptions.Count().Should().Be(2);
            }

            session1.Received(1).Dispose();
            session2.Received(1).Dispose();
            session3.Received(1).Dispose();
        }
    }
}