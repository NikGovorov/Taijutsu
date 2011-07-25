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

using NUnit.Framework;
using Taijutsu.Domain.Specs.Domain;

namespace Taijutsu.Domain.Specs
{
    // ReSharper disable InconsistentNaming
    public class EntityConversion_Specs
    {
        [Test]
        public virtual void As_extension_by_default_use_standard_native_as()
        {
            var order = (Order)new InternetOrder();
            var internetOrder = order.As<InternetOrder>();
            Assert.IsNotNull(internetOrder);
            Assert.IsTrue(order.Is<InternetOrder>());

            var order2 =  new Order();
            var internetOrder2 = order2.As<InternetOrder>();
            Assert.IsNull(internetOrder2);
            Assert.IsFalse(order2.Is<InternetOrder>());
        }
    }
    // ReSharper restore InconsistentNaming
}