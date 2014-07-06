﻿// Copyright 2009-2014 Nikita Govorov
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

using System.Data;

using NUnit.Framework;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DataSourceFixture : TestFixture
    {
        [TearDown]
        public void OnTearDown()
        {
            DataEnvironment.CheckDataContextSupervisorForRelease();
        }

        [Test]
        public virtual void ShouldAllowByDefaultRegisterDataSourcesWithSameName()
        {
            try
            {
                DataEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));
                DataEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));

                using (var uow = new UnitOfWork())
                {
                    Awaken(uow);
                }

                DataEnvironment.RegisterDataSource(new DataSource("test", IsolationLevel.RepeatableRead, il => new NullDataSession()));
                DataEnvironment.RegisterDataSource(new DataSource("test", il => new NullDataSession()));

                using (var uow = new UnitOfWork("test"))
                {
                    Awaken(uow);
                }

                Assert.That(
                    () => DataEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()), true), 
                    Throws.Exception.With.Message.EqualTo("Data source with name: '' has already been registered."));

                Assert.That(
                    () => DataEnvironment.RegisterDataSource(new DataSource("test", il => new NullDataSession()), true), 
                    Throws.Exception.With.Message.EqualTo("Data source with name: 'test' has already been registered."));
            }
            finally
            {
                DataEnvironment.UnregisterDataSource();
                DataEnvironment.UnregisterDataSource("test");
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Default data source is not registered.")]
        public virtual void ShouldRemoveDataSourceAfterUnregister()
        {
            DataEnvironment.RegisterDataSource(new DataSource(il => new NullDataSession()));
            DataEnvironment.UnregisterDataSource();
            using (new UnitOfWork())
            {
            }
        }
    }
}