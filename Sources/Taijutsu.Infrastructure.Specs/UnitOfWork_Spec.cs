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
using Taijutsu.Domain.Query;
using Taijutsu.Domain.Specs.Domain;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure.Specs
{
    // ReSharper disable InconsistentNaming
    public class UnitOfWork_Spec
    {
        [Test]
        public virtual void When_complete_has_not_been_called_rollback_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;

            provider.Expect(p => p.Commit()).Repeat.Never();
            provider.Expect(p => p.Rollback()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();

            using (new UnitOfWork())
            {
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        public virtual void When_complete_has_been_called_commit_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;

            provider.Expect(p => p.Commit()).Repeat.Once();
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();

            using (var uow = new UnitOfWork())
            {
                uow.Complete();
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        public virtual void When_exception_has_occured_close_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Close()).Repeat.Once();

            try
            {
                using (new UnitOfWork())
                {
                    throw new Exception();
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
            }
        }

        [Test]
        public virtual void When_exception_has_occured_before_complete_rollback_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();

            try
            {
                using (new UnitOfWork())
                {
                    throw new Exception();
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
            }
        }

        [Test]
        public virtual void When_exception_has_occured_after_complete_rollback_should_not_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Commit()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();

            try
            {
                using (var uow = new UnitOfWork())
                {
                    uow.Complete();
                    throw new Exception();
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
            }
        }

        [Test]
        [ExpectedException]
        public virtual void When_call_complete_more_than_once_exception_should_be_thrown()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;

            using (var uow = new UnitOfWork())
            {
                uow.Complete();
                uow.Complete();
            }
        }

        [Test]
        public virtual void When_exception_has_occured_during_commit_rollback_should_not_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Stub(p => p.Commit()).Throw(new Exception());

            try
            {
                using (var uow = new UnitOfWork())
                {
                    uow.Complete();
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
                return;
            }

            throw new NotSupportedException();
        }

        [Test]
        public virtual void When_call_complete_with_return_value_commit_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Expect(p => p.Commit()).Repeat.Once();


            using (var uow = new UnitOfWork())
            {
                Assert.AreEqual(1, uow.Complete(1));
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        public virtual void When_call_complete_with_return_callback_commit_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Expect(p => p.Commit()).Repeat.Once();


            using (var uow = new UnitOfWork())
            {
                Assert.AreEqual(1, uow.Complete(() => 1));
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        public virtual void When_call_complete_with_return_complex_callback_commit_should_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Expect(p => p.Commit()).Repeat.Once();


            using (var uow = new UnitOfWork())
            {
                Assert.AreEqual(1, uow.Complete(u => 1));
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        public virtual void When_call_complete_with_return_callback_with_exception_commit_should_not_be_called()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            provider.Expect(p => p.Rollback()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Expect(p => p.Commit()).Repeat.Never();


            try
            {
                using (var uow = new UnitOfWork())
                {
                    Assert.AreEqual(1, uow.Complete<int>(u => { throw new Exception(); }));
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
                return;
            }
            throw new NotSupportedException();
        }


        [Test]
        public virtual void When_unit_of_work_scope_completed_all_extensions_should_be_disposed()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;

            var ex1 = MockRepository.GenerateMock<IDisposable>();
            var ex2 = MockRepository.GenerateMock<IDisposable>();
            var ex3 = MockRepository.GenerateMock<IDisposable>();

            ex1.Expect(e => e.Dispose()).Repeat.Once();
            ex2.Expect(e => e.Dispose()).Repeat.Once();
            ex3.Expect(e => e.Dispose()).Repeat.Once();

            using (var uow = new UnitOfWork())
            {
                ((IAdvancedUnitOfWork) uow).Extension.Add("ex1", ex1);
                ((IAdvancedUnitOfWork) uow).Extension.Add("ex2", ex2);
                ((IAdvancedUnitOfWork) uow).Extension.Add("ex3", ex3);
            }
            provider.VerifyAllExpectations();
        }

        [Test]
        [ExpectedException(typeof (UnitOfWorkExtensionException))]
        public virtual void When_exception_occurs_in_one_extension_it_should_not_impact_uow_and_other_extension()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;

            provider.Expect(p => p.Rollback()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Expect(p => p.Commit()).Repeat.Never();

            var ex1 = MockRepository.GenerateMock<IDisposable>();
            var ex2 = MockRepository.GenerateMock<IDisposable>();
            var ex3 = MockRepository.GenerateMock<IDisposable>();

            ex1.Stub(e => e.Dispose()).Throw(new UnitOfWorkExtensionException());
            ex2.Expect(e => e.Dispose()).Repeat.Once();
            ex3.Expect(e => e.Dispose()).Repeat.Once();

            try
            {
                using (var uow = new UnitOfWork())
                {
                    ((IAdvancedUnitOfWork) uow).Extension.Add("ex1", ex1);
                    ((IAdvancedUnitOfWork) uow).Extension.Add("ex2", ex2);
                    ((IAdvancedUnitOfWork) uow).Extension.Add("ex3", ex3);
                }
            }
            catch (UnitOfWorkExtensionException)
            {
                provider.VerifyAllExpectations();
                throw;
            }
        }


        [Test]
        public virtual void Native_data_provider_should_be_accessible()
        {
            var provider = MockRepository.GenerateMock<DataProvider>();
            Internal.Infrastructure.DataProviderFactory = cfg => provider;
            var nhSession = new object();
            provider.Expect(p => p.NativeProvider).Return(nhSession);
            using (var uow = new UnitOfWork())
            {
                Assert.AreSame(nhSession, ((INativeUnitOf) uow).Native);
            }
        }

        [Test]
        [ExpectedException(typeof (QueryNotFoundException))]
        public virtual void All_delegating_methods_should_be_delegated()
        {
            var customer = new Customer(new FullName("Test", "Test"));
            var provider = MockRepository.GenerateMock<DataProvider>();
            try
            {
                Internal.Infrastructure.DataProviderFactory = cfg => provider;
                provider.Expect(p => p.AllOf<Customer>()).Repeat.Once();
                provider.Expect(p => p.UniqueOf<Customer>(default(Guid)))
                    .Return(MockRepository.GenerateStub<IQueryOfEntityByKey<Customer>>())
                    .Repeat.Once();

                provider.Expect(p => p.QueryOver<Customer>()).Throw(new QueryNotFoundException());

                provider.Expect(p => p.MarkAsCreated(customer)).Repeat.Twice().Return(customer.Key);
                provider.Expect(p => p.MarkAsRemoved(customer)).Repeat.Twice();

                using (var uow = new UnitOfWork())
                {
                    uow.AllOf<Customer>().Query();
                    uow.UniqueOf<Customer>(Guid.Empty).Query();
                    uow.Mark(customer).AsCreated();
                    uow.Mark(customer).AsRemoved();
                    uow.MarkAsCreated(customer);
                    uow.MarkAsRemoved(customer);
                    uow.Over<Customer>().By<GoldCustomerQuery>().Query();
                    uow.Complete();
                }
            }
            catch (QueryNotFoundException)
            {
                provider.VerifyAllExpectations();
                throw;
            }
            throw new NotSupportedException();
        }
    }

    // ReSharper restore InconsistentNaming

    internal class GoldCustomerQuery : IQueryOfEntity<Customer>
    {
        #region IQueryOfEntity<Customer> Members

        public Customer Query()
        {
            throw new NotImplementedException();
        }

        public IQueryOf<bool> Any
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryOfNotUniqueEntity<Customer> NotUniquely
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryOf<Customer> Strictly
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    internal class UnitOfWorkExtensionException : Exception
    {
    }

    internal class QueryNotFoundException : Exception
    {
    }
}