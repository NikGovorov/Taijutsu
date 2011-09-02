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
    [TestFixture]
    public class UnitOfWorkLevel_Specs
    {
        private string dataSource = string.Empty;

        [SetUp]
        protected void OnSetUp()
        {
            dataSource = Guid.NewGuid().ToString();
        }

        [Test]
        public virtual void Root_default_unit_of_work_should_include_all_default_sub_units_of_work()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));

            using (var uowL1I1 = new UnitOfWork(dataSource))
            {
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork(dataSource))
                {
                    Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                    uowL2I1.Complete();
                }

                using (var uowL2I2 = new UnitOfWork(dataSource))
                {
                    Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork(dataSource))
                    {
                        Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                        uowL3I1.Complete();
                    }

                    uowL2I2.Complete();
                }
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                uowL1I1.Complete();
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
            }
        }


        [Test]
        public virtual void After_scope_of_all_units_of_work_all_of_them_should_be_unregistered()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());
            var supervisor = Infrastructure.DataContextSupervisor;

            using (var uowL1I1 = new UnitOfWork(dataSource))
            {
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                Assert.AreSame(supervisor, Infrastructure.DataContextSupervisor);

                using (var uowL2I1 = new UnitOfWork(dataSource, Require.Existing))
                {
                }

                using (var uowL2I2 = new UnitOfWork(dataSource))
                {
                    using (var uowL3I1 = new UnitOfWork(dataSource, Require.Existing))
                    {
                    }
                }
                Assert.AreSame(supervisor, Infrastructure.DataContextSupervisor);
            }

            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());
            Assert.AreNotSame(supervisor, Infrastructure.DataContextSupervisor);
        }

        [Test]
        [ExpectedException]
        public virtual void Unit_of_work_with_require_existing_should_throw_exception_if_root_is_absent()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>()));
            using (var uowL1I1 = new UnitOfWork(dataSource, Require.Existing))
            {
            }
        }

        [Test]
        [ExpectedException]
        public virtual void Unit_of_work_with_require_existing_should_throw_exception_if_root_with_approp_source_is_absent()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), "w_crm_1"));
            using (var uowL1I1 = new UnitOfWork("w_crm_1"))
            {
                using (var uowL2I2 = new UnitOfWork("w_crm_1", Require.Existing)) //valid 
                {
                }

                using (var uowL2I2 = new UnitOfWork(dataSource, Require.Existing)) // not valid
                {
                }
            }
        }

        [Test]
        public virtual void Units_of_work_with_require_new_should_create_own_hierarchy()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());

            using (var uowL1I1 = new UnitOfWork(dataSource))
            {
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork(dataSource))
                {
                    Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());


                using (var uowL2I2 = new UnitOfWork(dataSource, Require.New))
                {
                    Assert.AreEqual(2, Infrastructure.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork(dataSource, Require.New))
                    {
                        Assert.AreEqual(3, Infrastructure.DataContextSupervisor.Roots.Count());
                        using (var uowL4I1 = new UnitOfWork(dataSource))
                        {
                            Assert.AreEqual(3, Infrastructure.DataContextSupervisor.Roots.Count());
                        }
                    }
                    Assert.AreEqual(2, Infrastructure.DataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());
        }

        [Test]
        public virtual void Units_of_work_with_defined_source_should_create_own_hierarchy()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), "w_crm_2"));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), "w_admin_2"));
            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());

            using (var uowL1I1 = new UnitOfWork("w_crm_2"))
            {
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork("w_crm_2"))
                {
                    Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());


                using (var uowL2I2 = new UnitOfWork("w_admin_2"))
                {
                    Assert.AreEqual(2, Infrastructure.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork(dataSource))
                    {
                        Assert.AreEqual(3, Infrastructure.DataContextSupervisor.Roots.Count());
                        using (var uowL4I1 = new UnitOfWork("w_admin_2"))
                        {
                            Assert.AreEqual(3, Infrastructure.DataContextSupervisor.Roots.Count());
                        }

                        using (var uowL4I2 = new UnitOfWork(dataSource, Require.New))
                        {
                            Assert.AreEqual(4, Infrastructure.DataContextSupervisor.Roots.Count());

                            using (var uowL5I1 = new UnitOfWork("w_crm_2"))
                            {
                                Assert.AreEqual(4, Infrastructure.DataContextSupervisor.Roots.Count());
                                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Where(u => u.SourceName == "w_crm_2").Count());
                            }
                            using (var uowL5I1 = new UnitOfWork("w_crm_2", Require.New))
                            {
                                Assert.AreEqual(5, Infrastructure.DataContextSupervisor.Roots.Count());
                                Assert.AreEqual(2, Infrastructure.DataContextSupervisor.Roots.Where(u => u.SourceName == "w_crm_2").Count());
                            }

                            using (var uowL5I1 = new UnitOfWork("w_admin_2"))
                            {
                                Assert.AreEqual(4, Infrastructure.DataContextSupervisor.Roots.Count());
                            }
                        }

                    }
                    Assert.AreEqual(2, Infrastructure.DataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, Infrastructure.DataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, Infrastructure.DataContextSupervisor.Roots.Count());
        }

        [Test]
        [ExpectedException]
        public virtual void Hierarchy_should_failed_if_not_all_sub_units_has_been_completed()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), "w_crm_3"));
            using (var uowL1I1 = new UnitOfWork("w_crm_3"))
            {
                using (var uowL2I2 = new UnitOfWork("w_crm_3"))
                {
                }

                uowL1I1.Complete();
            }
        }

        [Test]
        public virtual void Hierarchy_should_completed_if_all_sub_units_has_been_completed()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();

            provider.Expect(p => p.CommitTransaction()).Repeat.Once();
            provider.Expect(p => p.RollbackTransaction()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();

            var otherProvider = MockRepository.GenerateMock<DataProvider>();

            otherProvider.Expect(p => p.CommitTransaction()).Repeat.Never();
            otherProvider.Expect(p => p.RollbackTransaction()).Repeat.Twice();
            otherProvider.Expect(p => p.Close()).Repeat.Twice();

            bool follow = true;

            Infrastructure.RegisterDataSource(new LambdaDataSource(() => follow ? provider : otherProvider, dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => follow ? provider : otherProvider, "w_crm_4"));

            using (var uowL1I1 = new UnitOfWork("w_crm_4"))
            {
                follow = false;
                
                using (var uowL2I1 = new UnitOfWork(dataSource)) //another hierarchy
                {
                }
                
                follow = true;

                using (var uowL2I2 = new UnitOfWork("w_crm_4"))
                {
                    using (var uowL3I1 = new UnitOfWork("w_crm_4"))
                    {
                        uowL3I1.Complete();
                    }

                    uowL2I2.Complete();
                }

                follow = false;
                using (var uowL2I3 = new UnitOfWork("w_crm_4", Require.New)) //another hierarchy
                {
                    
                }
                uowL1I1.Complete();
            }
            provider.VerifyAllExpectations();
            otherProvider.VerifyAllExpectations();
        }


        
        [Test]
        [ExpectedException]
        public virtual void Hierarchy_should_not_allow_incompatible_isolation_level()
        {

            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            using (var uowL1I1 = new UnitOfWork(dataSource, IsolationLevel.ReadCommitted))
            {
                using (var uowL2I2 = new UnitOfWork(dataSource, IsolationLevel.Serializable))
                {
                    uowL2I2.Complete();
                }
                uowL1I1.Complete();
            }

        }

        [Test]
        public virtual void Hierarchy_should_be_compatible_with_isolation_level_with_root()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), "w_crm_5"));
            using (var uowL1I1 = new UnitOfWork("w_crm_5", IsolationLevel.RepeatableRead, Require.New))
            {
                using (var uowL2I2 = new UnitOfWork("w_crm_5", IsolationLevel.ReadCommitted))
                {
                    uowL2I2.Complete();
                }
                using (var uowL2I1 = new UnitOfWork(dataSource, IsolationLevel.Serializable)) //another hierarchy
                {
                    
                }

                uowL1I1.Complete();
            }

        }
    }
    // ReSharper restore InconsistentNaming
}