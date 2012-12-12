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

using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class SuppressionScopeFixture
    {
        [Test]
        public virtual void ShouldSupportNesting()
        {
            InternalEnvironment.IsInsideSuppressionScope.Should().Be.False();
            using (new SuppressionScope())
            {
                InternalEnvironment.IsInsideSuppressionScope.Should().Be.True();
                using (new SuppressionScope())
                {
                    InternalEnvironment.IsInsideSuppressionScope.Should().Be.True();
                }
                InternalEnvironment.IsInsideSuppressionScope.Should().Be.True();
            }
            InternalEnvironment.IsInsideSuppressionScope.Should().Be.False();
        }
    }
}