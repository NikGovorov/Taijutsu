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
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class DataContextFixture
    {
        private string source1;
        private string source2;

        [SetUp]
        protected void OnSetUp()
        {
            source1 = Guid.NewGuid().ToString();
            source2 = Guid.NewGuid().ToString();
            InternalEnvironment.RegisterDataSource(new DataSource(source1, il => new NullOrmSession()));
            InternalEnvironment.RegisterDataSource(new DataSource(source2, il => new NullOrmSession()));
        }

        [TearDown]
        protected void OnTearDown()
        {
            InternalEnvironment.UnregisterDataSource(source1);
            InternalEnvironment.UnregisterDataSource(source2);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Unit of work can not be successfully completed, because not all subordinates are completed.")]
        public virtual void ShouldThrowExceptionOnCompleteIfOneOfTheChildrenHasNotBeenCompleted()
        {
            using (var uowla = new UnitOfWork(source1))
            {
                using (var uowlai2 = new UnitOfWork(source1))
                {
                    using (new UnitOfWork(source1))
                    {
                    }

                    uowlai2.Complete();
                }

                uowla.Complete();
            }
        }
    }
}