#region License

//  Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using NUnit.Framework;
using SharpTestsEx;

namespace Taijutsu.Test
{
    public class ActionExFixture
    {
        [TestFixture]
        public class AsDisposableMethod
        {
            [Test]
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