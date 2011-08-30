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

using System;
using System.Threading;
using NUnit.Framework;

namespace Taijutsu.Specs.Domain
{
    // ReSharper disable InconsistentNaming    

    public class SeqGuid_Specs
    {
        [Test]
        public virtual void When_guid_has_been_generated_it_should_be_correlated_with_generation_date()
        {
            var dt1 = DateTime.Now;
            var id1 = SeqGuid.NewGuid();
            Thread.Sleep(100);
            var dt2 = DateTime.Now;
            var id2 = SeqGuid.NewGuid();
            Thread.Sleep(100);
            var dt3 = DateTime.Now;
            var id3 = SeqGuid.NewGuid();

            Assert.That(dt1, Is.EqualTo(SeqGuid.ToDateTime(id1)).Within(5).Milliseconds);
            Assert.That(dt2, Is.EqualTo(SeqGuid.ToDateTime(id2)).Within(5).Milliseconds);
            Assert.That(dt3, Is.EqualTo(SeqGuid.ToDateTime(id3)).Within(5).Milliseconds);

        }
    }

    // ReSharper restore InconsistentNaming
}