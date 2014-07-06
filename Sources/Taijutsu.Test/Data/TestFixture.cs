// Copyright 2009-2014 Nikita Govorov
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

using SharpTestsEx;

using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    public class TestFixture
    {
        protected virtual void Awaken(UnitOfWork uow)
        {
#pragma warning disable 168

            // ReSharper disable once UnusedVariable
            var nativeSession = ((IWrapper)uow).WrappedObject; // initialize lazy session
#pragma warning restore 168
        }

        protected virtual void Awaken(IDataContext context)
        {
#pragma warning disable 168

            // ReSharper disable once UnusedVariable
            var nativeSession = context.Session; // initialize lazy session
#pragma warning restore 168
        }

        protected virtual void AssertThatContextCountEqualTo(int count)
        {
            DataEnvironment.DataContextSupervisor.Contexts.Should().Have.Count.EqualTo(count);
        }

        protected virtual void AssertThatSupervisorDestroyed()
        {
            LogicContext.FindData("Taijutsu.DataContextSupervisor").Should().Be.Null();
        }
    }
}