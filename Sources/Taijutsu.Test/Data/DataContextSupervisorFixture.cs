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
using System.Data;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class DataContextSupervisorFixture : TestFixture
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
            InternalEnvironment.CheckDataContextSupervisorForRelease();
        }

        [Test]
        public virtual void RootShouldIncludeAllChildrenByDefault()
        {
            AssertThatContextCountEqualTo(0);

            using (new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (new UnitOfWork(source1))
                {
                    AssertThatContextCountEqualTo(1);

                    using (new UnitOfWork(source1))
                    {
                        AssertThatContextCountEqualTo(1);

                        using (var uow = new UnitOfWork(source1))
                        {
                            Awaken(uow);
                            AssertThatContextCountEqualTo(1);
                        }
                        using (new UnitOfWork(source1))
                        {
                            AssertThatContextCountEqualTo(1);
                        }

                        AssertThatContextCountEqualTo(1);
                    }

                    AssertThatContextCountEqualTo(1);
                }

                AssertThatContextCountEqualTo(1);
            }

            AssertThatSupervisorDestroyed();
        }

        [Test]
        public virtual void RootShouldIncludeAllChildrenWithRequireEqualToNone()
        {
            AssertThatContextCountEqualTo(0);

            using (new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (new UnitOfWork(source1, Require.None))
                {
                    AssertThatContextCountEqualTo(1);

                    using (new UnitOfWork(source1, Require.None))
                    {
                        AssertThatContextCountEqualTo(1);

                        using (new UnitOfWork(source1, Require.None))
                        {
                            AssertThatContextCountEqualTo(1);
                        }
                        using (new UnitOfWork(source1, Require.None))
                        {
                            AssertThatContextCountEqualTo(1);
                        }

                        AssertThatContextCountEqualTo(1);
                    }

                    AssertThatContextCountEqualTo(1);
                }

                AssertThatContextCountEqualTo(1);
            }

            AssertThatSupervisorDestroyed();
        }

        [Test]
        public virtual void RootShouldIncludeAllChildrenWithRequireEqualToExisting()
        {
            AssertThatContextCountEqualTo(0);

            using (new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (new UnitOfWork(source1, Require.Existing))
                {
                    AssertThatContextCountEqualTo(1);

                    using (new UnitOfWork(source1, Require.Existing))
                    {
                        AssertThatContextCountEqualTo(1);

                        using (new UnitOfWork(source1, Require.Existing))
                        {
                            AssertThatContextCountEqualTo(1);
                        }
                        using (new UnitOfWork(source1, Require.Existing))
                        {
                            AssertThatContextCountEqualTo(1);
                        }

                        AssertThatContextCountEqualTo(1);
                    }

                    AssertThatContextCountEqualTo(1);
                }

                AssertThatContextCountEqualTo(1);
            }

            AssertThatSupervisorDestroyed();
        }

        [Test]
        public virtual void RootShouldNotIncludeChildrenWithRequireEqualToNew()
        {
            AssertThatContextCountEqualTo(0);

            using (new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (var uow = new UnitOfWork(source1, Require.New))
                {
                    Awaken(uow);
                    AssertThatContextCountEqualTo(2);

                    using (new UnitOfWork(source1, Require.New))
                    {
                        AssertThatContextCountEqualTo(3);

                        using (new UnitOfWork(source1, Require.New))
                        {
                            AssertThatContextCountEqualTo(4);
                        }
                        using (new UnitOfWork(source1))
                        {
                            AssertThatContextCountEqualTo(3);
                        }

                        AssertThatContextCountEqualTo(3);
                    }

                    AssertThatContextCountEqualTo(2);
                }

                AssertThatContextCountEqualTo(1);
            }

            AssertThatSupervisorDestroyed();
        }

        [Test]
        public virtual void DifferentSourcesShouldCreateSeparateScopeHierarchy()
        {
            AssertThatContextCountEqualTo(0);

            using (new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (new UnitOfWork(source1, Require.New))
                {
                    AssertThatContextCountEqualTo(2);

                    using (new UnitOfWork(source2))
                    {
                        AssertThatContextCountEqualTo(3);

                        using (new UnitOfWork(source1))
                        {
                            AssertThatContextCountEqualTo(3);
                        }
                        using (new UnitOfWork(source2))
                        {
                            using (new UnitOfWork(source1))
                            {
                                using (new UnitOfWork(source2))
                                {
                                    AssertThatContextCountEqualTo(3);
                                }

                                AssertThatContextCountEqualTo(3);
                            }

                            AssertThatContextCountEqualTo(3);
                        }

                        AssertThatContextCountEqualTo(3);

                        using (new UnitOfWork(source2, Require.New))
                        {
                            AssertThatContextCountEqualTo(4);
                        }
                        AssertThatContextCountEqualTo(3);
                    }

                    AssertThatContextCountEqualTo(2);
                }

                AssertThatContextCountEqualTo(1);
            }

            AssertThatSupervisorDestroyed();

            AssertThatContextCountEqualTo(0);

            using (var uowla = new UnitOfWork(source1))
            {
                AssertThatContextCountEqualTo(1);

                using (var uowlai2 = new UnitOfWork(source1))
                {
                    AssertThatContextCountEqualTo(1);

                    using (new UnitOfWork(source2))
                    {
                        AssertThatContextCountEqualTo(2);
                    }
                    AssertThatContextCountEqualTo(1);

                    uowlai2.Complete();
                }
                AssertThatContextCountEqualTo(1);

                uowla.Complete();
            }
            AssertThatSupervisorDestroyed();
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Unit of work requires existing unit of work at the top level, but nothing has been found.")]
        public virtual void ShouldThrowExceptionForRequireEqualToExistingIfThereIsNoParentUnitOfWorks()
        {
            using (new UnitOfWork(source1, Require.Existing))
            {
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Isolation level 'Snapshot' is not compatible with 'Serializable'.")]
        public virtual void ShouldThrowExceptionIfIncompatibleIsolationLevelonTheSameLevelHasBeenDetected()
        {
            using (var uowla = new UnitOfWork(source1, IsolationLevel.Snapshot))
            {
                using (var uowlai2 = new UnitOfWork(source1, IsolationLevel.ReadCommitted))
                {
                    using (var uowlai3 = new UnitOfWork(source1, IsolationLevel.Serializable))
                    {
                        uowlai3.Complete();
                    }

                    uowlai2.Complete();
                }

                uowla.Complete();
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Data source with name 'oracedb' is not registered.")]
        public virtual void ShouldThrowExceptionIfDataSourceIsNotRegistered()
        {
            using (new UnitOfWork("oracedb"))
            {
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Default data source is not registered.")]
        public virtual void ShouldThrowExceptionIfDefaultDataSourceIsNotRegistered()
        {
            using (new UnitOfWork())
            {
            }
        }

        [Test]
        public virtual void ShouldExposeCurrentContext()
        {
            InternalEnvironment.DataContextSupervisor.Active.Should().Be.False();

            using (new UnitOfWork(source1))
            {
                InternalEnvironment.DataContextSupervisor.Active.Should().Be.True();

                var ctx1 = InternalEnvironment.DataContextSupervisor.Contexts.Single();
                ctx1.Should().Be.SameInstanceAs(InternalEnvironment.DataContextSupervisor.CurrentContext);

                using (new UnitOfWork(source1, Require.New))
                {
                    var ctx2 = InternalEnvironment.DataContextSupervisor.Contexts.Last();
                    ctx2.Should().Be.SameInstanceAs(InternalEnvironment.DataContextSupervisor.CurrentContext);

                    using (new UnitOfWork(source2))
                    {
                        var ctx3 = InternalEnvironment.DataContextSupervisor.Contexts.Last();
                        ctx3.Should().Be.SameInstanceAs(InternalEnvironment.DataContextSupervisor.CurrentContext);
                        InternalEnvironment.DataContextSupervisor.Active.Should().Be.True();
                    }
                    ctx2.Should().Be.SameInstanceAs(InternalEnvironment.DataContextSupervisor.CurrentContext);
                }

                ctx1.Should().Be.SameInstanceAs(InternalEnvironment.DataContextSupervisor.CurrentContext);
            }
            InternalEnvironment.DataContextSupervisor.CurrentContext.Should().Be.Null();
            InternalEnvironment.DataContextSupervisor.Active.Should().Be.False();
        }
    }
}