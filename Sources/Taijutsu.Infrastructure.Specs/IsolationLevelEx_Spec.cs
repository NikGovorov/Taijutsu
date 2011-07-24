// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Data;
using NUnit.Framework;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure.Specs
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class IsolationLevelEx_Spec
    {
        [Test]
        public virtual void Isolation_level_compatibility_should_be_detected_in_right_way()
        {
            Assert.IsTrue(IsolationLevel.Serializable.IsCompatible(IsolationLevel.ReadCommitted));
            Assert.IsTrue(IsolationLevel.ReadCommitted.IsCompatible(IsolationLevel.Unspecified));

            Assert.IsFalse(IsolationLevel.ReadCommitted.IsCompatible(IsolationLevel.Serializable));
            Assert.IsFalse(IsolationLevel.Snapshot.IsCompatible(IsolationLevel.Serializable));
            
            Assert.IsTrue(IsolationLevel.RepeatableRead.IsCompatible(IsolationLevel.Snapshot));
            Assert.IsTrue(IsolationLevel.Snapshot.IsCompatible(IsolationLevel.Snapshot));
            
            Assert.IsFalse(IsolationLevel.Snapshot.IsCompatible(IsolationLevel.RepeatableRead));
            Assert.IsFalse(IsolationLevel.Unspecified.IsCompatible(IsolationLevel.ReadCommitted));
        }
    }
    // ReSharper restore InconsistentNaming
}