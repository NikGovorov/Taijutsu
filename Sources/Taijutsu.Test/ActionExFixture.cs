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

using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;

namespace Taijutsu.Test
{
    [UsedImplicitly]
    public class ActionExFixture
    {
        [TestFixture]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class AsDisposableMethod
        {
            [Test]
            [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed. Acceptable for tests.")]
            public virtual void ShouldReturnActionWrappedWithDisposable()
            {
                var callCounter = 0;
                Action action = () => callCounter++;

                var disposable = action.AsDisposable();

                disposable.Dispose();
                disposable.Dispose();

                disposable = action.AsDisposable();

                disposable.Dispose();

                callCounter.Should().Be(2);
            }
        }
    }
}