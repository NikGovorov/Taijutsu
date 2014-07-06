// Copyright 2009-2014 Nikita Govorov
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

using System;
using System.Data;
using System.Linq;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DataContextSupervisorFixture : TestFixture
    {
        private string source1;

        private string source2;

        [SetUp]
        public void OnSetUp()
        {
            source1 = Guid.NewGuid().ToString();
            source2 = Guid.NewGuid().ToString();
            DataEnvironment.RegisterDataSource(new DataSource(source1, il => new NullDataSession()));
            DataEnvironment.RegisterDataSource(new DataSource(source2, il => new NullDataSession()));
        }

        [TearDown]
        public void OnTearDown()
        {
            DataEnvironment.UnregisterDataSource(source1);
            DataEnvironment.UnregisterDataSource(source2);
            DataEnvironment.CheckDataContextSupervisorForRelease();
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
            DataEnvironment.DataContextSupervisor.Active.Should().Be.False();

            using (new UnitOfWork(source1))
            {
                DataEnvironment.DataContextSupervisor.Active.Should().Be.True();

                var ctx1 = DataEnvironment.DataContextSupervisor.Contexts.Single();
                ctx1.Should().Be.SameInstanceAs(DataEnvironment.DataContextSupervisor.CurrentContext);

                using (new UnitOfWork(source1, Require.New))
                {
                    var ctx2 = DataEnvironment.DataContextSupervisor.Contexts.Last();
                    ctx2.Should().Be.SameInstanceAs(DataEnvironment.DataContextSupervisor.CurrentContext);

                    using (new UnitOfWork(source2))
                    {
                        var ctx3 = DataEnvironment.DataContextSupervisor.Contexts.Last();
                        ctx3.Should().Be.SameInstanceAs(DataEnvironment.DataContextSupervisor.CurrentContext);
                        DataEnvironment.DataContextSupervisor.Active.Should().Be.True();
                    }

                    ctx2.Should().Be.SameInstanceAs(DataEnvironment.DataContextSupervisor.CurrentContext);
                }

                ctx1.Should().Be.SameInstanceAs(DataEnvironment.DataContextSupervisor.CurrentContext);
            }

            DataEnvironment.DataContextSupervisor.CurrentContext.Should().Be.Null();
            DataEnvironment.DataContextSupervisor.Active.Should().Be.False();
        }
    }
}