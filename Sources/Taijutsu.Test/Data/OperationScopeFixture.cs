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

using NSubstitute;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class OperationScopeFixture
    {
        private string source;

        private IOrmSession session;

        [SetUp]
        public void OnSetUp()
        {
            source = Guid.NewGuid().ToString();
            session = Substitute.For<IOrmSession>();
            InternalEnvironment.RegisterDataSource(new DataSource(source, il => session));
        }

        [TearDown]
        public void OnTearDown()
        {
            InternalEnvironment.UnregisterDataSource(source);
            InternalEnvironment.UnregisterOperationScope();
            session.ClearReceivedCalls();
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Operation scope does not support nesting.")]
        public virtual void ShouldThrowExceptionIfNestingDetected()
        {
            using (new OperationScope())
            {
                using (new OperationScope())
                {
                }
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Operation scope can not be used inside of unit of work.")]
        public virtual void ShouldThrowExceptionIfOutsideUnitOfWorkDetected()
        {
            using (new UnitOfWork(source))
            {
                using (new OperationScope())
                {
                }
            }
        }

        [Test]
        public virtual void ShouldAllowSerialOperationScopes()
        {
            using (new OperationScope())
            {
                using (new UnitOfWork(source))
                {
                    using (new UnitOfWork(source))
                    {
                        InternalEnvironment.IsInsideOperationScope.Should().Be.True();
                    }
                }
            }

            InternalEnvironment.IsInsideOperationScope.Should().Be.False();

            using (new OperationScope())
            {
                using (new UnitOfWork(source))
                {
                    using (new UnitOfWork(source))
                    {
                        InternalEnvironment.IsInsideOperationScope.Should().Be.True();
                    }
                }
            }
        }
    }
}