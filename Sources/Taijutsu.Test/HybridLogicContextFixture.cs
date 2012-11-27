#region License

// Copyright 2009-2013 Nikita Govorov
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

using NSubstitute;
using NUnit.Framework;

namespace Taijutsu.Test
{
    [TestFixture]
    public class HybridLogicContextFixture
    {
        [Test]
        public virtual void ShouldDetermineContextForAppropriateEnvironment()
        {
            var context1 = Substitute.For<ILogicContext>();
            var context2 = Substitute.For<ILogicContext>();

            context1.IsApplicable().Returns(false);
            context2.IsApplicable().Returns(true);

            var hybridContext = (ILogicContext)new HybridLogicContext(new[] { context1, context2 });
            var data = new { };
            hybridContext.FindData("Test");
            hybridContext.SetData("Test", data);
            hybridContext.ReleaseData("Test");

            context1.Received(0).FindData("Test");
            context2.Received(1).FindData("Test");
            context1.Received(0).SetData("Test", data);
            context2.Received(1).SetData("Test", data);
            context1.Received(0).ReleaseData("Test");
            context2.Received(1).ReleaseData("Test");
        }

        [Test]
        public virtual void ShouldDetermineContextInOrderTheyHasBeenPassed()
        {
            var context1 = Substitute.For<ILogicContext>();
            var context2 = Substitute.For<ILogicContext>();
            
            context1.IsApplicable().Returns(true);
            context2.IsApplicable().Returns(true);

            var hybridContext = (ILogicContext)new HybridLogicContext(new[] { context1, context2 });

            hybridContext.FindData("Test");

            context1.Received(1).FindData("Test");
            context2.Received(0).FindData("Test");
        }

        [Test]
        public virtual void ShouldThrowExceptionIfNoneOfContextsAreApplicableToEnvironment()
        {
            var context1 = Substitute.For<ILogicContext>();
            var context2 = Substitute.For<ILogicContext>();

            context1.IsApplicable().Returns(false);
            context2.IsApplicable().Returns(false);

            var hybridContext = (ILogicContext)new HybridLogicContext(new[] { context1, context2 });

            Assert.That(() => hybridContext.FindData("Test"), Throws.Exception);
        }
    }
}