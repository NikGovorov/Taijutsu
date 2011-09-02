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
using NUnit.Framework;
using Rhino.Mocks;
using Taijutsu.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Specs.Data
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class OperationScope_Specs
    {
        #region Setup/Teardown

        [TearDown]
        protected virtual void OnTearDown()
        {
            Infrastructure.UnRegisterOperationScope();
        }

        #endregion


        private string dataSource = string.Empty;

        [SetUp]
        protected void OnSetUp()
        {
            dataSource = Guid.NewGuid().ToString();
        }

        private class DisposingException : Exception
        {
        }

        [Test]
        public virtual void All_units_should_be_closed_after_operation_scope_had_been_closed()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            var readOnlyProvider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => provider, () => readOnlyProvider, dataSource));
            

            provider.Expect(p => p.Close()).Repeat.Once();
            readOnlyProvider.Expect(p => p.Close()).Repeat.Once();


            using (new OperationScope())
            {
                using (new UnitOfWork(dataSource))
                {
                    using (new UnitOfQuery(dataSource))
                    {
                    }
                }
            }

            provider.VerifyAllExpectations();
            readOnlyProvider.VerifyAllExpectations();
        }

        [Test]
        public virtual void All_units_should_not_be_closed_since_operation_scope_open()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            var readOnlyProvider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => provider, () => readOnlyProvider, dataSource));

            provider.Expect(p => p.Close()).Repeat.Never();
            readOnlyProvider.Expect(p => p.Close()).Repeat.Never();

            // ReSharper disable UnusedVariable
            var scope = new OperationScope();
            // ReSharper restore UnusedVariable

            using (new UnitOfWork(dataSource))
            {
                using (new UnitOfQuery(dataSource))
                {
                }
            }

            provider.VerifyAllExpectations();
            readOnlyProvider.VerifyAllExpectations();
        }

        [Test]
        [ExpectedException(typeof (DisposingException))]
        public virtual void Error_during_closing_one_unit_in_operation_should_affect_other_units()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            var readOnlyProvider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => provider, () => readOnlyProvider, dataSource));
            try
            {
                provider.Stub(p => p.Close()).Throw(new DisposingException());
                readOnlyProvider.Expect(p => p.Close()).Repeat.Once();


                using (new OperationScope())
                {
                    using (new UnitOfWork(dataSource))
                    {
                        using (new UnitOfQuery(dataSource))
                        {
                        }
                    }
                }
            }
            catch (DisposingException)
            {
                provider.VerifyAllExpectations();
                readOnlyProvider.VerifyAllExpectations();
                throw;
            }
        }

        [Test]
        [ExpectedException]
        public virtual void Only_one_scope_is_allowed_simultaneously()
        {
            using (new OperationScope())
            {
                using (new OperationScope())
                {
                }
            }
        }


        [Test]
        public virtual void Scope_and_unit_can_start_after_another_finished()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            var readOnlyProvider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => provider, () => readOnlyProvider, dataSource));

            using (new UnitOfQuery(dataSource))
            {
            }

            using (new UnitOfWork(dataSource))
            {
            }

            using (new OperationScope())
            {
            }

            using (new UnitOfWork(dataSource))
            {
            }
            using (new UnitOfQuery(dataSource))
            {
            }
            using (new OperationScope())
            {
                using (new UnitOfQuery(dataSource))
                {
                }
            }
            using (new UnitOfQuery(dataSource))
            {
                using (var uow = new UnitOfWork(dataSource))
                {
                    uow.Complete();
                }
            }
            using (new OperationScope())
            {
                using (var uow = new UnitOfWork(dataSource))
                {
                    uow.Complete();
                }
            }
            using (new UnitOfQuery(dataSource))
            {
            }
        }

        [Test]
        public virtual void Scope_can_start_after_another_finished()
        {
            using (new OperationScope())
            {
            }

            using (new OperationScope())
            {
            }

            using (new OperationScope())
            {
            }
        }


        [Test]
        [ExpectedException]
        public virtual void Unit_of_query_scope_cant_include_op_scope()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<ReadOnlyDataProvider>(), dataSource));

            using (new UnitOfQuery(dataSource))
            {
                using (new OperationScope())
                {
                }
            }
        }

        [Test]
        [ExpectedException]
        public virtual void Unit_of_work_scope_cant_include_op_scope()
        {
            Infrastructure.RegisterDataSource(new LambdaDataSource(() => MockRepository.GenerateStub<DataProvider>(), dataSource));

            using (new UnitOfWork(dataSource))
            {
                using (new OperationScope())
                {
                }
            }
        }
    }

    // ReSharper restore InconsistentNaming
}