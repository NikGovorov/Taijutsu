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

using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Taijutsu.Infrastructure.Internal;
using System.Linq;

namespace Taijutsu.Infrastructure.Specs
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class UnitOfWorkLevel_Specs
    {
        [Test]
        public virtual void Root_default_unit_of_work_should_include_all_default_sub_units_of_work()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();

            using (var uowL1I1 = new UnitOfWork())
            {
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork())
                {
                    Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                    uowL2I1.Complete();
                }

                using (var uowL2I2 = new UnitOfWork())
                {
                    Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork())
                    {
                        Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                        uowL3I1.Complete();
                    }

                    uowL2I2.Complete();
                }
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                uowL1I1.Complete();
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
            }
        }


        [Test]
        public virtual void After_scope_of_all_units_of_work_all_of_them_should_be_unregistered()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());
            var supervisor = SupervisorContext.DataContextSupervisor;

            using (var uowL1I1 = new UnitOfWork())
            {
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                Assert.AreSame(supervisor, SupervisorContext.DataContextSupervisor);

                using (var uowL2I1 = new UnitOfWork(Require.Existing))
                {
                }

                using (var uowL2I2 = new UnitOfWork())
                {
                    using (var uowL3I1 = new UnitOfWork(Require.Existing))
                    {
                    }
                }
                Assert.AreSame(supervisor, SupervisorContext.DataContextSupervisor);
            }

            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());
            Assert.AreNotSame(supervisor, SupervisorContext.DataContextSupervisor);
        }

        [Test]
        [ExpectedException]
        public virtual void Unit_of_work_with_require_existing_should_throw_exception_if_root_is_absent()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            using (var uowL1I1 = new UnitOfWork(Require.Existing))
            {
            }
        }

        [Test]
        [ExpectedException]
        public virtual void Unit_of_work_with_require_existing_should_throw_exception_if_root_with_approp_source_is_absent()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            using (var uowL1I1 = new UnitOfWork("crm"))
            {
                using (var uowL2I2 = new UnitOfWork("crm", Require.Existing)) //valid 
                {
                }

                using (var uowL2I2 = new UnitOfWork(Require.Existing)) // not valid
                {
                }
            }
        }

        [Test]
        public virtual void Units_of_work_with_require_new_should_create_own_hierarchy()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());

            using (var uowL1I1 = new UnitOfWork())
            {
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork())
                {
                    Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());


                using (var uowL2I2 = new UnitOfWork(Require.New))
                {
                    Assert.AreEqual(2, SupervisorContext.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork(Require.New))
                    {
                        Assert.AreEqual(3, SupervisorContext.DataContextSupervisor.Roots.Count());
                        using (var uowL4I1 = new UnitOfWork())
                        {
                            Assert.AreEqual(3, SupervisorContext.DataContextSupervisor.Roots.Count());
                        }
                    }
                    Assert.AreEqual(2, SupervisorContext.DataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());
        }

        [Test]
        public virtual void Units_of_work_with_defined_source_should_create_own_hierarchy()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());

            using (var uowL1I1 = new UnitOfWork("crm"))
            {
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());

                using (var uowL2I1 = new UnitOfWork("crm"))
                {
                    Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
                }

                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());


                using (var uowL2I2 = new UnitOfWork("admin"))
                {
                    Assert.AreEqual(2, SupervisorContext.DataContextSupervisor.Roots.Count());
                    using (var uowL3I1 = new UnitOfWork())
                    {
                        Assert.AreEqual(3, SupervisorContext.DataContextSupervisor.Roots.Count());
                        using (var uowL4I1 = new UnitOfWork("admin"))
                        {
                            Assert.AreEqual(3, SupervisorContext.DataContextSupervisor.Roots.Count());
                        }

                        using (var uowL4I2 = new UnitOfWork(Require.New))
                        {
                            Assert.AreEqual(4, SupervisorContext.DataContextSupervisor.Roots.Count());
                            
                            using (var uowL5I1 = new UnitOfWork("crm"))
                            {
                                Assert.AreEqual(4, SupervisorContext.DataContextSupervisor.Roots.Count());
                                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Where(u=>u.SourceName=="crm").Count());
                            }
                            using (var uowL5I1 = new UnitOfWork("crm", Require.New))
                            {
                                Assert.AreEqual(5, SupervisorContext.DataContextSupervisor.Roots.Count());
                                Assert.AreEqual(2, SupervisorContext.DataContextSupervisor.Roots.Where(u => u.SourceName == "crm").Count());
                            }

                            using (var uowL5I1 = new UnitOfWork("admin"))
                            {
                                Assert.AreEqual(4, SupervisorContext.DataContextSupervisor.Roots.Count());
                            }
                        }

                    }
                    Assert.AreEqual(2, SupervisorContext.DataContextSupervisor.Roots.Count());
                }
                Assert.AreEqual(1, SupervisorContext.DataContextSupervisor.Roots.Count());
            }
            Assert.AreEqual(0, SupervisorContext.DataContextSupervisor.Roots.Count());
        }

        [Test]
        [ExpectedException]
        public virtual void Hierarchy_should_failed_if_not_all_sub_units_has_been_completed()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            using (var uowL1I1 = new UnitOfWork("crm"))
            {
                using (var uowL2I2 = new UnitOfWork("crm"))
                {
                }

                uowL1I1.Complete();
            }
        }

        [Test]
        public virtual void Hierarchy_should_completed_if_all_sub_units_has_been_completed()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();

            provider.Expect(p => p.Commit()).Repeat.Once();
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();

            var otherProvider = MockRepository.GenerateMock<DataProvider>();

            otherProvider.Expect(p => p.Commit()).Repeat.Never();
            otherProvider.Expect(p => p.Rollback()).Repeat.Twice();
            otherProvider.Expect(p => p.Close()).Repeat.Twice();

            bool follow = true;

            Internal.Infrastructure.DataProviderFactory = cfg => follow ? provider : otherProvider;

            using (var uowL1I1 = new UnitOfWork("crm"))
            {
                follow = false;
                
                using (var uowL2I1 = new UnitOfWork()) //another hierarchy
                {
                }
                
                follow = true;
                
                using (var uowL2I2 = new UnitOfWork("crm"))
                {
                    using (var uowL3I1 = new UnitOfWork("crm"))
                    {
                        uowL3I1.Complete();
                    }

                    uowL2I2.Complete();
                }

                follow = false;
                using (var uowL2I3 = new UnitOfWork("crm", Require.New)) //another hierarchy
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

            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            using (var uowL1I1 = new UnitOfWork(IsolationLevel.ReadCommitted))
            {
                using (var uowL2I2 = new UnitOfWork(IsolationLevel.Serializable))
                {
                    uowL2I2.Complete();
                }
                uowL1I1.Complete();
            }

        }

        [Test]
        public virtual void Hierarchy_should_be_compatible_with_isolation_level_with_root()
        {
            Internal.Infrastructure.DataProviderFactory = cfg => MockRepository.GenerateStub<DataProvider>();
            using (var uowL1I1 = new UnitOfWork("crm", IsolationLevel.RepeatableRead, Require.New))
            {
                using (var uowL2I2 = new UnitOfWork("crm", IsolationLevel.ReadCommitted))
                {
                    uowL2I2.Complete();
                }
                using (var uowL2I1 = new UnitOfWork(IsolationLevel.Serializable)) //another hierarchy
                {
                    
                }

                uowL1I1.Complete();
            }

        }
    }
    // ReSharper restore InconsistentNaming
}