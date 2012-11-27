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

using NUnit.Framework;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class ValueObjectFixture
    {

        [Test]
        public virtual void ShouldBeComparedByInternalState()
        {
            var name1 = new FullName("Test", "MegaTest");
            var name2 = new FullName("Test", "MegaTest");
            var name2AsObject = (object)name2;

            var name3 = new FullName("Test2", "OmegaTest");
            var name4 = new ExFullName("Test2", "OmegaTest", "khsadf");

            Assert.IsTrue(name1 == name2);
            Assert.IsTrue(name1.GetHashCode() == name2.GetHashCode());
            Assert.IsTrue(name1.Equals(name2AsObject));

            Assert.IsTrue(name2 != name3);
            Assert.IsTrue(name1 != name3);
            Assert.IsTrue(name1.GetHashCode() != name3.GetHashCode());

            Assert.IsFalse(name1.Equals(null));
            Assert.IsTrue(null != name3);
            Assert.IsTrue(name4 != name3);
            Assert.IsTrue(name4.GetHashCode() != name3.GetHashCode());


            var name5 = new FullNameMirror("Test", "MegaTest");
            var name6 = new FullNameAbsoluteMirror("Test", "MegaTest");

            Assert.IsTrue(name5.Equals(name1));
            Assert.IsTrue(name6.Equals(name1));
        }

        public class ExFullName : FullName
        {
            protected string anotherName;

            public ExFullName(string firstName, string secondName, string anotherName)
                : base(firstName, secondName)
            {
                this.anotherName = anotherName;
            }

            public virtual string AnotherName
            {
                get { return anotherName; }
            }

            protected override object BuildIdentity()
            {
                return new {firstName, secondName, anotherName};
            }
        }

        public class FullNameMirror : FullName
        {
            public FullNameMirror(string firstName, string secondName)
                : base(firstName, secondName)
            {
            }

            protected override object BuildIdentity()
            {
                return new {firstName, secondName};
            }
        }

        public class FullNameAbsoluteMirror : FullName
        {
            public FullNameAbsoluteMirror(string firstName, string secondName)
                : base(firstName, secondName)
            {
            }
        }
    }
}