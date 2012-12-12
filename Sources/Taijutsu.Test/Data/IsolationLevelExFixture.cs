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

using System.Data;
using NUnit.Framework;
using Taijutsu.Data.Internal;
using SharpTestsEx;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class IsolationLevelExFixture
    {
        [Test]
        public virtual void IsolationLevelCompatibilityShouldBeDeterminedCorrectly()
        {
            IsolationLevel.Serializable.IsCompatible(IsolationLevel.ReadUncommitted).Should().Be.True();
            
            IsolationLevel.Serializable.IsCompatible(IsolationLevel.Snapshot).Should().Be.False();
            
            IsolationLevel.ReadCommitted.IsCompatible(IsolationLevel.Unspecified).Should().Be.True();
            
            IsolationLevel.ReadCommitted.IsCompatible(IsolationLevel.Serializable).Should().Be.False();
            
            IsolationLevel.RepeatableRead.IsCompatible(IsolationLevel.Snapshot).Should().Be.True();

            IsolationLevel.Snapshot.IsCompatible(IsolationLevel.Serializable).Should().Be.False();

            IsolationLevel.Snapshot.IsCompatible(IsolationLevel.Snapshot).Should().Be.True();

            IsolationLevel.Snapshot.IsCompatible(IsolationLevel.RepeatableRead).Should().Be.False();

            IsolationLevel.Unspecified.IsCompatible(IsolationLevel.ReadCommitted).Should().Be.False();
        }
    }
}