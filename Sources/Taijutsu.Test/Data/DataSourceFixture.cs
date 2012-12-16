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
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class DataSourceFixture: TestFixture
    {
        [TearDown]
        protected void OnTearDown()
        {
            InternalEnvironment.CheckDataContextSupervisorForRelease();
        }


        [Test]
        public virtual void ShouldAllowByDefaultRegisterDataSourcesWithSameName()
        {
            try
            {
                InternalEnvironment.RegisterDataSource(new DataSource(il => new NullOrmSession()));
                InternalEnvironment.RegisterDataSource(new DataSource(il => new NullOrmSession()));
                
                using (var uow = new UnitOfWork())
                {
                    Awaken(uow);
                }

                InternalEnvironment.RegisterDataSource(new DataSource("test", il => new NullOrmSession()));
                InternalEnvironment.RegisterDataSource(new DataSource("test", il => new NullOrmSession()));

                using (var uow = new UnitOfWork("test"))
                {
                    Awaken(uow);
                }

                Assert.That(() => InternalEnvironment.RegisterDataSource(new DataSource(il => new NullOrmSession()), true),
                    Throws.Exception.With.Message.EqualTo("Data source with name: '' has already been registered."));

                Assert.That(() => InternalEnvironment.RegisterDataSource(new DataSource("test", il => new NullOrmSession()), true),
                    Throws.Exception.With.Message.EqualTo("Data source with name: 'test' has already been registered."));
            }
            finally
            {
                InternalEnvironment.UnregisterDataSource();
                InternalEnvironment.UnregisterDataSource("test");
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Default data source is not registered.")]
        public virtual void ShouldRemoveDataSourceAfterUnregister()
        {
            InternalEnvironment.RegisterDataSource(new DataSource(il => new NullOrmSession()));
            InternalEnvironment.UnregisterDataSource();
            using (new UnitOfWork())
            {
                
            }
        }
    }
}