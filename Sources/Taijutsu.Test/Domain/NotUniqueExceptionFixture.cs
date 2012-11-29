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
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    public class NotUniqueExceptionFixture
    {
        [Test]
        public virtual void ShouldHaveAppropriateMessage()
        {
            var ex1 = new NotUniqueException(100, typeof (InternetOrder));
            var ex2 = new NotUniqueException(100, "Internet Order");
            var ex3 = new NotUniqueException("Gold Orders", typeof (InternetOrder));
            var ex4 = new NotUniqueException("Gold Orders", "Internet Order");
            var ex5 = new NotUniqueException<InternetOrder>(100);
            var ex6 = new NotUniqueException<InternetOrder>("Silver Orders");

            ex1.Message.Should().Be("Entity with '100' id and 'Taijutsu.Test.Domain.Model.InternetOrder' type is not unique.");

            ex2.Message.Should().Be("Entity with '100' id and 'Internet Order' type is not unique.");

            ex3.Message.Should().Be("Entity of 'Taijutsu.Test.Domain.Model.InternetOrder' type is not unique. Query requires unique results. Query description: 'Gold Orders'.");

            ex4.Message.Should().Be("Entity of 'Internet Order' type is not unique. Query requires unique results. Query description: 'Gold Orders'.");

            ex5.Message.Should().Be("Entity with '100' id and 'Taijutsu.Test.Domain.Model.InternetOrder' type is not unique.");

            ex6.Message.Should().Be("Entity of 'Taijutsu.Test.Domain.Model.InternetOrder' type is not unique. Query requires unique results. Query description: 'Silver Orders'.");
        }

        [Test]
        public virtual void ShouldHaveAppropriateIdAndType()
        {
            var ex1 = new NotUniqueException(100, typeof (InternetOrder));
            var ex2 = new NotUniqueException(100, "Internet Order");
            var ex3 = new NotUniqueException("Gold Orders", typeof (InternetOrder));
            var ex4 = new NotUniqueException("Gold Orders", "Internet Order");
            var ex5 = new NotUniqueException<InternetOrder>(100);
            var ex6 = new NotUniqueException<InternetOrder>("Silver Orders");

            ex1.Id.Should().Be(100);
            ex1.Type.Should().Be(typeof (InternetOrder));
            ex2.Id.Should().Be(100);
            ex2.Type.Should().Be("Internet Order");
            ex3.Id.Should().Be("unknown");
            ex3.Type.Should().Be(typeof (InternetOrder));
            ex4.Id.Should().Be("unknown");
            ex4.Type.Should().Be("Internet Order");
            ex5.Id.Should().Be(100);
            ex5.Type.Should().Be(typeof (InternetOrder));
            ex6.Id.Should().Be("unknown");
            ex6.Type.Should().Be(typeof (InternetOrder));
        }


        [Test]
        public virtual void ShouldHaveAppropriateInnerException()
        {
            var innerException = new Exception("Test Exception");
            var ex1 = new NotUniqueException(100, typeof (InternetOrder), innerException);
            var ex2 = new NotUniqueException(100, "Internet Order", innerException);
            var ex3 = new NotUniqueException("Gold Orders", typeof (InternetOrder), innerException);
            var ex4 = new NotUniqueException("Gold Orders", "Internet Order", innerException);
            var ex5 = new NotUniqueException<InternetOrder>(100, innerException);
            var ex6 = new NotUniqueException<InternetOrder>("Silver Orders", innerException);

            ex1.InnerException.Should().Be.SameInstanceAs(innerException);
            ex2.InnerException.Should().Be.SameInstanceAs(innerException);
            ex3.InnerException.Should().Be.SameInstanceAs(innerException);
            ex4.InnerException.Should().Be.SameInstanceAs(innerException);
            ex5.InnerException.Should().Be.SameInstanceAs(innerException);
            ex6.InnerException.Should().Be.SameInstanceAs(innerException);
        }
    }
}
