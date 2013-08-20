// Copyright 2009-2013 Nikita Govorov
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

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Domain;
using Taijutsu.Test.Domain.Model;

namespace Taijutsu.Test.Domain
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EntityConversionFixture
    {
        [TearDown]
        public void OnTearDown()
        {
            EntityConversionRegistry.CustomizeWith(new IEntityConversion[] { });
        }

        [Test]
        public virtual void ShouldUseNativeConversionByDefault()
        {
            var order = (Order)new InternetOrder();
            var internetOrder = order.As<InternetOrder>();
            Assert.IsNotNull(internetOrder);
            Assert.IsTrue(order.Is<InternetOrder>());

            var order2 = new Order();
            var internetOrder2 = order2.As<InternetOrder>();
            Assert.IsNull(internetOrder2);
            Assert.IsFalse(order2.Is<InternetOrder>());

            EntityConversionRegistry.NativeConversion.IsApplicableFor(new object()).Should().Be.True();
            EntityConversionRegistry.NativeConversion.IsApplicableFor(new InternetOrder()).Should().Be.True();
        }

        [Test]
        public virtual void ShouldBeCustomizable()
        {
            EntityConversionRegistry.CustomizeWith(new[] { new CustomConversion() });

            var order = (Order)new InternetOrder();
            var internetOrder = order.As<InternetOrder>();

            Assert.IsNull(internetOrder);
            Assert.IsFalse(order.Is<InternetOrder>());
        }

        private class CustomConversion : IEntityConversion
        {
            public bool IsApplicableFor(object entity)
            {
                return entity is Order;
            }

            public T SafeConvert<T>(object entity) where T : class
            {
                return null;
            }
        }
    }
}