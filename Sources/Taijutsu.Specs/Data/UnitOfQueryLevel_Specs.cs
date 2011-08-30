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
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Specs.Data
{
    // ReSharper disable InconsistentNaming
    public class UnitOfQueryLevel_Specs
    {
        private string dataSource = string.Empty;

        [SetUp]
        protected void OnSetUp()
        {
            dataSource = Guid.NewGuid().ToString();
        }


        [Test]
        public virtual void Root_default_unit_of_query_should_include_all_default_sub_units_of_query()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));

            using (var uoqL1I1 = new UnitOfQuery(dataSource))
            {
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

                using (var uoqL2I1 = new UnitOfQuery(dataSource))
                {
                    Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                }

                using (var uoqL2I2 = new UnitOfQuery(dataSource))
                {
                    Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                    using (var uoqL3I1 = new UnitOfQuery(dataSource))
                    {
                        Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                    }
                }
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
            }
        }

        [Test]
        public virtual void After_scope_of_all_units_of_query_all_of_them_should_be_unregistered()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
            var supervisor = SupervisorContext.ReadOnlyDataContextSupervisor;

            using (var uoqL1I1 = new UnitOfQuery(dataSource))
            {
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                Assert.AreSame(supervisor, SupervisorContext.ReadOnlyDataContextSupervisor);

                using (var uoqL2I1 = new UnitOfQuery(dataSource, Require.Existing))
                {
                }

                using (var uoqL2I2 = new UnitOfQuery(dataSource))
                {
                    using (var uoqL3I1 = new UnitOfQuery(dataSource, Require.Existing))
                    {
                    }
                }
                Assert.AreSame(supervisor, SupervisorContext.ReadOnlyDataContextSupervisor);
            }

            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
            Assert.AreNotSame(supervisor, SupervisorContext.ReadOnlyDataContextSupervisor);
        }


        [Test]
        [ExpectedException]
        public virtual void Unit_of_query_with_require_existing_should_throw_exception_if_root_is_absent()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            using (var uoqL1I1 = new UnitOfQuery(dataSource, Require.Existing))
            {
            }
        }


        [Test]
        [ExpectedException]
        public virtual void Unit_of_query_with_require_existing_should_throw_exception_if_root_with_approp_source_is_absent()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), "q_crm_01"));
            using (var uoqL1I1 = new UnitOfQuery("q_crm_01"))
            {
                using (var uoqL2I2 = new UnitOfQuery("q_crm_01", Require.Existing)) //valid 
                {
                }

                using (var uoqL2I2 = new UnitOfQuery(dataSource, Require.Existing)) // not valid
                {
                }
            }
        }


        [Test]
        public virtual void Units_of_query_with_require_new_should_create_own_hierarchy()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

            using (var uoqL1I1 = new UnitOfQuery(dataSource))
            {
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

                using (var uoqL2I1 = new UnitOfQuery(dataSource))
                {
                    Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());


                using (var uoqL2I2 = new UnitOfQuery(dataSource, Require.New))
                {
                    Assert.AreEqual(2, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                    using (var uoqL3I1 = new UnitOfQuery(dataSource, Require.New))
                    {
                        Assert.AreEqual(3, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                        using (var uoqL4I1 = new UnitOfQuery(dataSource))
                        {
                            Assert.AreEqual(3, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                        }
                    }
                    Assert.AreEqual(2, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
        }


        [Test]
        public virtual void Units_of_query_with_defined_source_should_create_own_hierarchy()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), "q_crm_2"));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), "q_admin_2"));

            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

            using (var uoqL1I1 = new UnitOfQuery("q_crm_2"))
            {
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

                using (var uoqL2I1 = new UnitOfQuery("q_crm_2"))
                {
                    Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());


                using (var uoqL2I2 = new UnitOfQuery("q_admin_2"))
                {
                    Assert.AreEqual(2, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                    using (var uoqL3I1 = new UnitOfQuery(dataSource))
                    {
                        Assert.AreEqual(3, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                        using (var uoqL4I1 = new UnitOfQuery("q_admin_2"))
                        {
                            Assert.AreEqual(3, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                        }

                        using (var uoqL4I2 = new UnitOfQuery(dataSource, Require.New))
                        {
                            Assert.AreEqual(4, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());

                            using (var uoqL5I1 = new UnitOfQuery("q_crm_2"))
                            {
                                Assert.AreEqual(4, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Where(u => u.SourceName == "q_crm_2").Count());
                            }
                            using (var uoqL5I1 = new UnitOfQuery("q_crm_2", Require.New))
                            {
                                Assert.AreEqual(5, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                                Assert.AreEqual(2, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Where(u => u.SourceName == "q_crm_2").Count());
                            }

                            using (var uoqL5I1 = new UnitOfQuery("q_admin_2"))
                            {
                                Assert.AreEqual(4, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                            }
                        }

                    }
                    Assert.AreEqual(2, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, SupervisorContext.ReadOnlyDataContextSupervisor.Roots.Count());
        }


        [Test]
        [ExpectedException]
        public virtual void Hierarchy_should_not_allow_incompatible_isolation_level()
        {

            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            using (var uoqL1I1 = new UnitOfQuery(dataSource, IsolationLevel.ReadCommitted))
            {
                using (var uoqL2I2 = new UnitOfQuery(dataSource, IsolationLevel.Serializable))
                {
                }
            }

        }

        [Test]
        public virtual void Hierarchy_should_be_compatible_with_isolation_level_with_root()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), "q_crm_3"));
            using (var uoqL1I1 = new UnitOfQuery("q_crm_3", IsolationLevel.RepeatableRead, Require.New))
            {
                using (var uoqL2I2 = new UnitOfQuery("q_crm_3", IsolationLevel.ReadCommitted))
                {
                }
                using (var uoqL2I1 = new UnitOfQuery(dataSource, IsolationLevel.Serializable)) //another hierarchy
                {

                }
            }

        }
    }
    // ReSharper restore InconsistentNaming
}